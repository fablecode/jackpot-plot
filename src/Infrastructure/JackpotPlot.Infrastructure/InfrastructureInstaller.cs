using JackpotPlot.Application.Abstractions.Common;
using JackpotPlot.Application.Abstractions.Messaging;
using JackpotPlot.Infrastructure.Common;
using JackpotPlot.Infrastructure.Jobs;
using JackpotPlot.Infrastructure.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace JackpotPlot.Infrastructure
{
    public static class InfrastructureInstaller
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            return services
                .AddLotteryDataCollectorServiceApplicationServices()
                .AddMessagingServices();
        }

        public static IServiceCollection AddMessagingServices(this IServiceCollection services)
        {
            services.AddSingleton(typeof(IQueueWriter<>), typeof(RabbitMqQueueWriter<>));
            services.AddSingleton(typeof(IQueueReader<>), typeof(RabbitMqQueueReader<>));
            services.AddSingleton<IRandomProvider, DefaultRandomProvider>();

            return services;
        }

        public static IServiceCollection AddLotteryDataCollectorServiceApplicationServices(this IServiceCollection services)
        {
            return services
                .AddJobs()
                .AddMediatR(cfg
                    => cfg.RegisterServicesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies()));
        }

        private static IServiceCollection AddJobs(this IServiceCollection services)
        {
            return services.AddTransient<EurojackpotJobs>();
        }
    }
}
