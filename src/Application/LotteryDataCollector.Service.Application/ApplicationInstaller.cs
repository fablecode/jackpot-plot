using JackpotPlot.Domain.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace LotteryDataCollector.Service.Application;

public static class ApplicationInstaller
{
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