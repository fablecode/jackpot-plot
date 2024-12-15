using JackpotPlot.Domain.Repositories;
using JackpotPlot.Infrastructure;
using JackpotPlot.Prediction.API.Infrastructure.Databases;
using JackpotPlot.Prediction.API.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JackpotPlot.Prediction.API.Infrastructure
{
    public static class InfrastructureInstaller
    {
        public static IServiceCollection AddPredictionApiInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            return services
                .AddMessagingServices()
                .AddDatabase(configuration)
                .AddRepositories();
        }

        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddTransient<ILotteryHistoryRepository, LotteryHistoryRepository>();

            return services;
        }
        public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContextFactory<PredictionDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("PredictionApiDatabase")));

            return services;
        }
    }
}
