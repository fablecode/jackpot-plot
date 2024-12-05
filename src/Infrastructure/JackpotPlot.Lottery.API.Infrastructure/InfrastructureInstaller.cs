using JackpotPlot.Domain.Repositories;
using JackpotPlot.Infrastructure;
using JackpotPlot.Infrastructure.Repositories;
using JackpotPlot.Lottery.API.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace JackpotPlot.Lottery.API.Infrastructure
{
    public static class InfrastructureInstaller
    {
        public static IServiceCollection AddLotteryApiInfrastructureServices(this IServiceCollection services)
        {
            return services
                .AddMessagingServices()
                .AddRepositories();
        }

        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddTransient<ILotteryRepository, LotteryRepository>();
            services.AddTransient<IDrawRepository, DrawRepository>();

            return services;
        }
    }
}
