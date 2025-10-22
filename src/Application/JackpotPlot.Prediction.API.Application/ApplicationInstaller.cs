using FluentValidation;
using JackpotPlot.Domain.Predictions;
using JackpotPlot.Domain.Predictions.Algorithms;
using JackpotPlot.Primitives.Algorithms;
using Microsoft.Extensions.DependencyInjection;

namespace JackpotPlot.Prediction.API.Application;

public static class ApplicationInstaller
{
    public static IServiceCollection AddPredictionApiApplicationServices(this IServiceCollection services)
    {
        return services
            .AddValidations()
            .AddAlgorithms()
            .AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies()));
    }

    public static IServiceCollection AddValidations(this IServiceCollection services)
    {
        return services
            .AddValidatorsFromAssemblyContaining(typeof(ApplicationInstaller), ServiceLifetime.Transient);
    }

    public static IServiceCollection AddAlgorithms(this IServiceCollection services)
    {
        // 1: one-to-one algorithms (keyed DI)
        services.AddKeyedScoped<IPredictionAlgorithm, ClusteringAnalysisAlgorithm>(PredictionAlgorithmKeys.ClusteringAnalysis);
        services.AddKeyedScoped<IPredictionAlgorithm, ConsecutiveNumbersAlgorithm>(PredictionAlgorithmKeys.ConsecutiveNumbers);
        services.AddKeyedScoped<IPredictionAlgorithm, CyclicPatternsAlgorithm>(PredictionAlgorithmKeys.CyclicPatterns);
        services.AddKeyedScoped<IPredictionAlgorithm, DeltaSystemAlgorithm>(PredictionAlgorithmKeys.DeltaSystem);
        services.AddKeyedScoped<IPredictionAlgorithm, DrawPositionAnalysisAlgorithm>(PredictionAlgorithmKeys.DrawPositionAnalysis);
        services.AddKeyedScoped<IPredictionAlgorithm, FrequencyAlgorithm>(PredictionAlgorithmKeys.FrequencyBased);
        services.AddKeyedScoped<IPredictionAlgorithm, GapAnalysisAlgorithm>(PredictionAlgorithmKeys.GapAnalysis);
        services.AddKeyedScoped<IPredictionAlgorithm, GroupSelectionAlgorithm>(PredictionAlgorithmKeys.GroupSelection);
        services.AddKeyedScoped<IPredictionAlgorithm, HighLowNumberSplitAlgorithm>(PredictionAlgorithmKeys.HighLowNumberSplit);
        services.AddKeyedScoped<IPredictionAlgorithm, InvertedFrequencyAlgorithm>(PredictionAlgorithmKeys.InvertedFrequency);
        services.AddKeyedScoped<IPredictionAlgorithm, LastAppearanceAlgorithm>(PredictionAlgorithmKeys.LastAppearance);
        services.AddKeyedScoped<IPredictionAlgorithm, NumberChainAlgorithm>(PredictionAlgorithmKeys.NumberChain);
        services.AddKeyedScoped<IPredictionAlgorithm, NumberSumAlgorithm>(PredictionAlgorithmKeys.NumberSum);
        services.AddKeyedScoped<IPredictionAlgorithm, OddEvenBalanceAlgorithm>(PredictionAlgorithmKeys.OddEvenBalance);
        services.AddKeyedScoped<IPredictionAlgorithm, PatternMatchingAlgorithm>(PredictionAlgorithmKeys.PatternMatching);
        services.AddKeyedScoped<IPredictionAlgorithm, QuadrantAnalysisAlgorithm>(PredictionAlgorithmKeys.QuadrantAnalysis);
        services.AddKeyedScoped<IPredictionAlgorithm, RandomAlgorithm>(PredictionAlgorithmKeys.Random);
        services.AddKeyedScoped<IPredictionAlgorithm, RarePatternsAlgorithm>(PredictionAlgorithmKeys.RarePatterns);
        services.AddKeyedScoped<IPredictionAlgorithm, ReducedNumberPoolAlgorithm>(PredictionAlgorithmKeys.ReducedNumberPool);
        services.AddKeyedScoped<IPredictionAlgorithm, RepeatingNumbersAlgorithm>(PredictionAlgorithmKeys.RepeatingNumbers);
        services.AddKeyedScoped<IPredictionAlgorithm, SeasonalPatternsAlgorithm>(PredictionAlgorithmKeys.SeasonalPatterns);
        services.AddKeyedScoped<IPredictionAlgorithm, SkewnessAnalysisAlgorithm>(PredictionAlgorithmKeys.SkewnessAnalysis);
        services.AddKeyedScoped<IPredictionAlgorithm, StandardDeviationAlgorithm>(PredictionAlgorithmKeys.StandardDeviation);
        services.AddKeyedScoped<IPredictionAlgorithm, StatisticalAveragingAlgorithm>(PredictionAlgorithmKeys.StatisticalAveraging);
        services.AddKeyedScoped<IPredictionAlgorithm, SymmetryAnalysisAlgorithm>(PredictionAlgorithmKeys.SymmetryAnalysis);
        services.AddKeyedScoped<IPredictionAlgorithm, TimeDecayAlgorithm>(PredictionAlgorithmKeys.TimeDecay);
        services.AddKeyedScoped<IPredictionAlgorithm, WeightDistributionAlgorithm>(PredictionAlgorithmKeys.WeightDistribution);
        services.AddKeyedScoped<IPredictionAlgorithm, WeightedProbabilityAlgorithm>(PredictionAlgorithmKeys.WeightedProbability);

        // 2: Mixed algorithm (composes other keyed algos) — factory registration
        services.AddKeyedScoped<IPredictionAlgorithm>(PredictionAlgorithmKeys.Mixed, (sp, _) =>
        {
            // Example weights — swap to IOptions if you want config-driven values
            var weights = new Dictionary<string, double>
            {
                { PredictionAlgorithmKeys.FrequencyBased,     0.5 },
                { PredictionAlgorithmKeys.InvertedFrequency,  0.2 },
                { PredictionAlgorithmKeys.GapAnalysis,        0.1 },
                { PredictionAlgorithmKeys.HighLowNumberSplit, 0.1 },
                { PredictionAlgorithmKeys.OddEvenBalance,     0.1 },
            };

            var components = new List<(IPredictionAlgorithm Algo, double Weight)>
            {
                (sp.GetRequiredKeyedService<IPredictionAlgorithm>(PredictionAlgorithmKeys.FrequencyBased),     weights[PredictionAlgorithmKeys.FrequencyBased]),
                (sp.GetRequiredKeyedService<IPredictionAlgorithm>(PredictionAlgorithmKeys.InvertedFrequency),  weights[PredictionAlgorithmKeys.InvertedFrequency]),
                (sp.GetRequiredKeyedService<IPredictionAlgorithm>(PredictionAlgorithmKeys.GapAnalysis),        weights[PredictionAlgorithmKeys.GapAnalysis]),
                (sp.GetRequiredKeyedService<IPredictionAlgorithm>(PredictionAlgorithmKeys.HighLowNumberSplit), weights[PredictionAlgorithmKeys.HighLowNumberSplit]),
                (sp.GetRequiredKeyedService<IPredictionAlgorithm>(PredictionAlgorithmKeys.OddEvenBalance),     weights[PredictionAlgorithmKeys.OddEvenBalance]),
            };

            return new MixedAlgorithm(components);
        });

        return services;
    }
}