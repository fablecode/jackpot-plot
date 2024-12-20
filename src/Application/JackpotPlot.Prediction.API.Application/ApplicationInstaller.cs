﻿using FluentValidation;
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
        services.AddScoped<IPredictionStrategy, ClusteringAnalysisPredictionStrategy>();
        services.AddScoped<IPredictionStrategy, ConsecutiveNumbersPredictionStrategy>();
        services.AddScoped<IPredictionStrategy, DeltaSystemPredictionStrategy>();
        services.AddScoped<IPredictionStrategy, FrequencyPredictionStrategy>();
        services.AddScoped<IPredictionStrategy, HighLowNumberSplitPredictionStrategy>();
        services.AddScoped<IPredictionStrategy, LastAppearancePredictionStrategy>();
        services.AddScoped<IPredictionStrategy, OddEvenBalancePredictionStrategy>();
        services.AddScoped<IPredictionStrategy, PatternMatchingPredictionStrategy>();
        services.AddScoped<IPredictionStrategy, RandomPredictionStrategy>();
        services.AddScoped<IPredictionStrategy, StatisticalAveragingPredictionStrategy>();
        services.AddScoped<IPredictionStrategy, WeightedProbabilityPredictionStrategy>();

        // Mathematical and Statistical Strategies
        services.AddScoped<IPredictionStrategy, NumberSumPredictionStrategy>();
        services.AddScoped<IPredictionStrategy, GapAnalysisPredictionStrategy>();
        services.AddScoped<IPredictionStrategy, SkewnessAnalysisPredictionStrategy>();
        services.AddScoped<IPredictionStrategy, StandardDeviationPredictionStrategy>();
        services.AddScoped<IPredictionStrategy, WeightDistributionPredictionStrategy>();
        services.AddScoped<IPredictionStrategy, QuadrantAnalysisPredictionStrategy>();

        // Time-Based and Temporal Strategies
        services.AddScoped<IPredictionStrategy, TimeDecayPredictionStrategy>();
        services.AddScoped<IPredictionStrategy, SeasonalPatternsPredictionStrategy>();
        services.AddScoped<IPredictionStrategy, CyclicPatternsPredictionStrategy>();
        services.AddScoped<IPredictionStrategy, DrawPositionAnalysisPredictionStrategy>();

        // Combination-Based Strategies
        services.AddScoped<IPredictionStrategy, ReducedNumberPoolPredictionStrategy>();
        services.AddScoped<IPredictionStrategy, GroupSelectionPredictionStrategy>();

        return services;
    }
}