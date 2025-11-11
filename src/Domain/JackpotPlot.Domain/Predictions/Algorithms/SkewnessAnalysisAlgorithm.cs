using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.ValueObjects;
using System.Collections.Immutable;
using JackpotPlot.Domain.Predictions.Helpers;
using JackpotPlot.Primitives.Algorithms;

namespace JackpotPlot.Domain.Predictions.Algorithms;

[PredictionAlgorithmDescription(PredictionAlgorithmKeys.SkewnessAnalysis, "sUses the skewness of the distribution of historical draw numbers to determine if the draw is biased toward higher or lower numbers, and predicts accordingly to match that trend.")]
public sealed class SkewnessAnalysisAlgorithm : IPredictionAlgorithm
{
    public PredictionResult Predict(
        LotteryConfigurationDomain config,
        IReadOnlyList<HistoricalDraw> history,
        Random rng)
    {
        if (history.Count == 0)
        {
            return new PredictionResult(
                config.LotteryId,
                ImmutableArray<int>.Empty,
                ImmutableArray<int>.Empty,
                0d,
                PredictionAlgorithmKeys.SkewnessAnalysis);
        }

        // 1) Analyze skewness from all historical main numbers
        var skewness = SkewnessAnalysisAlgorithmHelpers.CalculateSkewness(history);

        // 2) Generate main numbers based on skewness signal
        var main = SkewnessAnalysisAlgorithmHelpers.GenerateNumbersBasedOnSkewness(
                maxRange: config.MainNumbersRange,
                count: config.MainNumbersCount,
                skewness: skewness,
                rng: rng)
            .ToImmutableArray();

        // 3) Bonus numbers (random, distinct from main)
        var bonus = config.BonusNumbersCount > 0
            ? SkewnessAnalysisAlgorithmHelpers.RandomDistinct(1, config.BonusNumbersRange, main, config.BonusNumbersCount, rng)
            : ImmutableArray<int>.Empty;

        // 4) Confidence (same spirit as original: closer mean + larger |skew| ↓ confidence)
        var confidence = SkewnessAnalysisAlgorithmHelpers.CalculateSkewnessConfidence(history, main);

        return new PredictionResult(
            config.LotteryId,
            main,
            bonus,
            confidence,
            PredictionAlgorithmKeys.SkewnessAnalysis);
    }
}