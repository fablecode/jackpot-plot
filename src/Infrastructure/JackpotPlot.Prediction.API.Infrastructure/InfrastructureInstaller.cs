using JackpotPlot.Infrastructure;
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
            //services.AddTransient<ILotteryRepository, LotteryRepository>();
            //services.AddTransient<IDrawRepository, DrawRepository>();
            //services.AddTransient<IDrawResultRepository, DrawResultRepository>();

            return services;
        }
        public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            //services.AddDbContextFactory<LotteryDbContext>(options =>
            //    options.UseNpgsql(configuration.GetConnectionString("LotteryApiDatabase")));

            return services;
        }
    }
}
