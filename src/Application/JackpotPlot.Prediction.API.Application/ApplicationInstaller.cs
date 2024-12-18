using FluentValidation;
using JackpotPlot.Domain.Interfaces;
using JackpotPlot.Domain.Services.PredictionStrategies;
using Microsoft.Extensions.DependencyInjection;

namespace JackpotPlot.Prediction.API.Application;

public static class ApplicationInstaller
{
    public static IServiceCollection AddPredictionApiApplicationServices(this IServiceCollection services)
    {
        return services
            .AddValidations()
            .AddStrategies()
            .AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies()));
    }

    public static IServiceCollection AddValidations(this IServiceCollection services)
    {
        return services
            .AddValidatorsFromAssemblyContaining(typeof(ApplicationInstaller), ServiceLifetime.Transient);
    }
    public static IServiceCollection AddStrategies(this IServiceCollection services)
    {
        services.AddScoped<IPredictionStrategy, FrequencyPredictionStrategy>();
        services.AddScoped<IPredictionStrategy, RandomPredictionStrategy>();

        return services;
    }
}