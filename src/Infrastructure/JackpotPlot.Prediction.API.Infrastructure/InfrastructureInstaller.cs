using JackpotPlot.Application.Abstractions.Messaging;
using JackpotPlot.Application.Abstractions.Persistence.Repositories;
using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Settings;
using JackpotPlot.Infrastructure;
using JackpotPlot.Infrastructure.Messaging;
using JackpotPlot.Prediction.API.Infrastructure.Databases;
using JackpotPlot.Prediction.API.Infrastructure.Repositories;
using JackpotPlot.Prediction.API.Infrastructure.Services.LotteryApi;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(baseUrl ?? throw new ArgumentNullException(nameof(baseUrl))));

            return services;
        }

        public static IServiceCollection AddHostedServices(this IServiceCollection services)
        {
            // Register the background service that will consume RabbitMQ messages
            services.AddMassTransit(x =>
            {
                x.AddConsumer<QueueToMediatorConsumer<Message<LotteryDrawnEvent>>>();

                // Optional: global endpoint naming for auto-mapped endpoints elsewhere
                x.SetKebabCaseEndpointNameFormatter();

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.UseNewtonsoftJsonSerializer();
                    cfg.ConfigureNewtonsoftJsonSerializer(s =>
                    {
                        s.NullValueHandling = NullValueHandling.Ignore;
                        s.DateParseHandling = DateParseHandling.DateTimeOffset;
                        return s;
                    });
                    cfg.ConfigureNewtonsoftJsonDeserializer(s =>
                    {
                        s.NullValueHandling = NullValueHandling.Ignore;
                        return s;
                    });

                    var settings = context.GetRequiredService<IOptions<RabbitMqSettings>>().Value;

                    cfg.Host(settings.Host, h =>
                    {
                        h.Username(settings.Username);
                        h.Password(settings.Password);
                    });

                    // Avoid per-message exchanges from this process
                    cfg.Publish<Message<LotteryDrawnEvent>>(topologyConfigurator => topologyConfigurator.Exclude = true);

                    cfg.ReceiveEndpoint("lottery-db-update", e =>
                    {
                        e.ConfigureConsumeTopology = false;

                        // HA / performance knobs
                        e.PrefetchCount = 1;               // tune
                        e.ConcurrentMessageLimit = 1;       // tune

                        // Bind to your topic exchange with explicit keys
                        e.Bind(settings.Exchange, b =>
                        {
                            b.ExchangeType = "topic";
                            b.RoutingKey = $"{RoutingKeys.LotteryDbUpdate}.{EventTypes.LotteryDrawn}";
                            b.Durable = true;
                        });

                        // Resiliency
                        e.UseMessageRetry(r => r.Exponential(5, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(3)));

                        // Consumer
                        e.ConfigureConsumer<QueueToMediatorConsumer<Message<LotteryDrawnEvent>>>(context);
                    });
                });
            });


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
