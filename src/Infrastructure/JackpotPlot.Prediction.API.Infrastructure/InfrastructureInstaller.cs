using JackpotPlot.Application.Abstractions.Messaging;
using JackpotPlot.Application.Abstractions.Persistence.Repositories;
using JackpotPlot.Domain.Models;
using JackpotPlot.Infrastructure;
using JackpotPlot.Prediction.API.Infrastructure.Databases;
using JackpotPlot.Prediction.API.Infrastructure.HostedServices;
using JackpotPlot.Prediction.API.Infrastructure.Repositories;
using JackpotPlot.Prediction.API.Infrastructure.Services.LotteryApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Refit;

namespace JackpotPlot.Prediction.API.Infrastructure
{
    public static class InfrastructureInstaller
    {
        public static IServiceCollection AddPredictionApiInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            return services
                .AddHostedServices()
                .AddMessagingServices()
                .AddDatabase(configuration)
                .AddRepositories()
                .AddServices(configuration);
        }

        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddTransient<ILotteryHistoryRepository, LotteryHistoryRepository>();
            services.AddTransient<ILotteryConfigurationRepository, LotteryConfigurationRepository>();
            services.AddTransient<IPredictionRepository, PredictionRepository>();
            services.AddTransient<ILotteryStatisticsRepository, LotteryStatisticsRepository>();

            return services;
        }
        public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContextFactory<PredictionDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("PredictionApiDatabase")));

            return services;
        }
        public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
        {
            var baseUrl = configuration.GetValue<string>("ApiSettings:LotteryServiceUrl");

            var refitSettings = new RefitSettings
            {
                ContentSerializer = new NewtonsoftJsonContentSerializer(new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    NullValueHandling = NullValueHandling.Ignore
                })
            };

            services.AddRefitClient<ILotteryService>(refitSettings)
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(baseUrl));

            return services;
        }

        public static IServiceCollection AddHostedServices(this IServiceCollection services)
        {
            services.AddHostedService<LotteryDrawnBackgroundService<Message<LotteryDrawnEvent>>>();

            return services;
        }
    }

    public static class RefitExtensions
    {
        public static T ForRefit<T>(string domainUrl)
        {
            return RestService.For<T>(domainUrl,
                new RefitSettings
                {
                    ContentSerializer = new NewtonsoftJsonContentSerializer(
                        new JsonSerializerSettings()
                        {
                            ContractResolver = new CamelCasePropertyNamesContractResolver(),
                            Converters = { new StringEnumConverter() }
                        }
                    )
                });
        }
    }
}
