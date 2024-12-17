using JackpotPlot.Domain.Repositories;
using JackpotPlot.Infrastructure;
using JackpotPlot.Lottery.API.Infrastructure.Databases;
using JackpotPlot.Lottery.API.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JackpotPlot.Lottery.API.Infrastructure
{
    public static class InfrastructureInstaller
    {
        public static IServiceCollection AddLotteryApiInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            return services
                .AddMessagingServices()
                .AddDatabase(configuration)
                .AddRepositories();
        }

        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddTransient<ILotteryRepository, LotteryRepository>();
            services.AddTransient<IDrawRepository, DrawRepository>();
            services.AddTransient<IDrawResultRepository, DrawResultRepository>();
            services.AddTransient<ILotteryConfigurationRepository, LotteryConfigurationRepository>();

            return services;
        }
        public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContextFactory<LotteryDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("LotteryApiDatabase")));

            return services;
        }
    }
}
