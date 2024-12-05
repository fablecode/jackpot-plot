using JackpotPlot.Domain.Messaging;
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
            services.AddSingleton(typeof(IQueueWriter<>), typeof(RabbitMqQueueWriter<>));
            services.AddSingleton(typeof(IQueueReader<>), typeof(RabbitMqQueueReader<>));

            return services;
        }
    }
}
