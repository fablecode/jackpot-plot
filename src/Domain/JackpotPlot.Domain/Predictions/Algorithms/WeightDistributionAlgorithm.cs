using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.ValueObjects;
using System.Collections.Immutable;
using JackpotPlot.Domain.Predictions.Helpers;
using JackpotPlot.Primitives.Algorithms;

namespace JackpotPlot.Domain.Predictions.Algorithms;

[PredictionAlgorithmDescription(PredictionAlgorithmKeys.WeightDistribution, "Similar to weighted probability, this strategy calculates and applies weights to numbers based on historical data to select numbers proportionally from their weight distribution.")]
public sealed class WeightDistributionAlgorithm : IPredictionAlgorithm
{
    public PredictionResult Predict(
        LotteryConfigurationDomain config,
        IReadOnlyList<HistoricalDraw> history,
        Random rng)
    {
        // no history → empty (or fallback to random if you prefer)
        if (history.Count == 0)
        {
            return new PredictionResult(
                config.LotteryId,
                ImmutableArray<int>.Empty,
                ImmutableArray<int>.Empty,
                0d,
                PredictionAlgorithmKeys.WeightDistribution);
        }

        // 1) weights over the main-number range
        var weights = WeightDistributionAlgorithmHelpers.CalculateWeights(history, config.MainNumbersRange);

        // 2) sample main numbers by weight, without replacement
        var main = WeightDistributionAlgorithmHelpers.GenerateWeightedRandomNumbers(weights, config.MainNumbersCount, rng)
            .ToImmutableArray();

        // 3) bonus numbers (random & distinct from main)
        var bonus = config.BonusNumbersCount > 0
            ? WeightDistributionAlgorithmHelpers.RandomDistinct(1, config.BonusNumbersRange, main, config.BonusNumbersCount, rng)
            : ImmutableArray<int>.Empty;

        // 4) confidence: same overlap ratio as the original strategy
        var confidence = WeightDistributionAlgorithmHelpers.CalculateWeightDistributionConfidence(history, main.ToList());

        return new PredictionResult(
            config.LotteryId, main, bonus, confidence, PredictionAlgorithmKeys.WeightDistribution);
    }
}