using Microsoft.Extensions.DependencyInjection;

namespace JackpotPlot.Application;

public static class ApplicationInstaller
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        return services
            .AddMediatR(cfg
                => cfg.RegisterServicesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies()));
    }
}