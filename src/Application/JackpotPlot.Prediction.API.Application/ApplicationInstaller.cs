using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace JackpotPlot.Prediction.API.Application;

public static class ApplicationInstaller
{
    public static IServiceCollection AddPredictionApiApplicationServices(this IServiceCollection services)
    {
        return services
            .AddValidations()
            .AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies()));
    }

    public static IServiceCollection AddValidations(this IServiceCollection services)
    {
        return services
            .AddValidatorsFromAssemblyContaining(typeof(ApplicationInstaller), ServiceLifetime.Transient);
    }
}