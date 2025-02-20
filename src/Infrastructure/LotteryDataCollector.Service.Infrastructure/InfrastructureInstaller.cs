using JackpotPlot.Domain.Services;
using JackpotPlot.Infrastructure;
using LotteryDataCollector.Service.Infrastructure.Services;
using LotteryDataCollector.Service.Infrastructure.WebPages;
using Microsoft.Extensions.DependencyInjection;

namespace LotteryDataCollector.Service.Infrastructure
{
    public static class InfrastructureInstaller
    {
        public static IServiceCollection AddLotteryDataCollectorServiceInfrastructureServices(this IServiceCollection services)
        {
            services.AddTransient<IHtmlWebPage, HtmlWebPage>();

            services.AddHttpClient();

            return services
                .AddDomainServices()
                .AddMessagingServices();
        }

        public static IServiceCollection AddDomainServices(this IServiceCollection services)
        {
            services.AddTransient<IEurojackpotService, EurojackpotOrgService>();

            return services;
        }
    }
}
