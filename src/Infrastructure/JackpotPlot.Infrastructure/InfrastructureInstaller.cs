using JackpotPlot.Domain.Messaging;
using JackpotPlot.Domain.Services;
using JackpotPlot.Infrastructure.Messaging;
using JackpotPlot.Infrastructure.Services;
using JackpotPlot.Infrastructure.WebPages;
using Microsoft.Extensions.DependencyInjection;

namespace JackpotPlot.Infrastructure
{
    public static class InfrastructureInstaller
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            services.AddTransient<IHtmlWebPage, HtmlWebPage>();

            return services
                .AddDomainServices()
                .AddMessagingServices();
        }

        public static IServiceCollection AddDomainServices(this IServiceCollection services)
        {
            services.AddTransient<IEurojackpotService, EurojackpotService>();

            return services;
        }
        public static IServiceCollection AddMessagingServices(this IServiceCollection services)
        {
            services.AddScoped(typeof(IQueue<>), typeof(RabbitMqQueue<>));

            return services;
        }
    }
}
