using JackpotPlot.Domain.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace JackpotPlot.Application;

public static class ApplicationInstaller
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        return services
            .AddDomainModels()
            .AddMediatR(cfg
                => cfg.RegisterServicesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies()));
    }

    private static IServiceCollection AddDomainModels(this IServiceCollection services)
    {
        return services.AddTransient<EurojackpotJobs>();
    }
}