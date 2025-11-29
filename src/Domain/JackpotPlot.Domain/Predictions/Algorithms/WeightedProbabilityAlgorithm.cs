using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.ValueObjects;
using System.Collections.Immutable;
using JackpotPlot.Domain.Predictions.Helpers;
using JackpotPlot.Primitives.Algorithms;

namespace JackpotPlot.Domain.Predictions.Algorithms;

[PredictionAlgorithmDescription(PredictionAlgorithmKeys.WeightedProbability, "Assigns weights to each number based on its historical frequency and selects numbers through weighted random sampling, favoring those with higher or lower weights as needed.")]
public sealed class WeightedProbabilityAlgorithm : IPredictionAlgorithm
{
    public PredictionResult Predict(
        LotteryConfigurationDomain config,
        IReadOnlyList<HistoricalDraw> history,
        Random rng)
    {
        // No history → empty (or swap to uniform random if you prefer)
        if (history.Count == 0)
        {
            return new PredictionResult(
                config.LotteryId,
                ImmutableArray<int>.Empty,
                ImmutableArray<int>.Empty,
                0d,
                PredictionAlgorithmKeys.WeightedProbability);
        }

        // 1) weights over [1..MainNumbersRange]
        var weights = WeightedProbabilityAlgorithmHelpers.CalculateWeights(history, config.MainNumbersRange);

        // 2) main numbers: weighted sample without replacement
        var main = WeightedProbabilityAlgorithmHelpers.WeightedSampleDistinct(weights, config.MainNumbersCount, rng).ToImmutableArray();

        // 3) bonus numbers: uniform random & distinct from main
        var bonus = config.BonusNumbersCount > 0
            ? WeightedProbabilityAlgorithmHelpers.RandomDistinct(1, config.BonusNumbersRange, main, config.BonusNumbersCount, rng)
            : ImmutableArray<int>.Empty;

        // 4) confidence: historical overlap weighted by probabilities
        var confidence = WeightedProbabilityAlgorithmHelpers.CalculateWeightedProbabilityConfidence(history, weights);

        return new PredictionResult(
            config.LotteryId,
            main,
            bonus,
            confidence,
            PredictionAlgorithmKeys.WeightedProbability);
    }
}