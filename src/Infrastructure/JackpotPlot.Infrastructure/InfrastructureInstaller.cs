using JackpotPlot.Application.Abstractions.Common;
using JackpotPlot.Application.Abstractions.Messaging;
using JackpotPlot.Infrastructure.Common;
using JackpotPlot.Infrastructure.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace JackpotPlot.Infrastructure
{
    public static class InfrastructureInstaller
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            return services
                .AddMessagingServices();
        }

        public static IServiceCollection AddMessagingServices(this IServiceCollection services)
        {
            services.AddSingleton(typeof(IQueueWriter<>), typeof(MassTransitExchangeWriter<>));
            services.AddSingleton<IRandomProvider, DefaultRandomProvider>();

            return services;
        }

        public static IServiceCollection AddLotteryDataCollectorServiceApplicationServices(this IServiceCollection services)
        {
            return services
                .AddMediatR(cfg
                    => cfg.RegisterServicesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies()));
        }
    }
}
