using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.ValueObjects;
using System.Collections.Immutable;
using JackpotPlot.Domain.Predictions.Helpers;
using JackpotPlot.Primitives.Algorithms;

namespace JackpotPlot.Domain.Predictions.Algorithms;

[PredictionAlgorithmDescription(PredictionAlgorithmKeys.NumberSum, "Focuses on the overall sum of numbers in past draws. It calculates an average (or target) sum from historical data and selects numbers that, when combined, approximate this total.")]
public sealed class NumberSumAlgorithm : IPredictionAlgorithm
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
                0,
                PredictionAlgorithmKeys.NumberSum);
        }

        // 1) compute target sum (historical average of main-number sums)
        var targetSum = NumberSumAlgorithmHelpers.CalculateTargetSum(history);

        // 2) generate main numbers near the target sum
        var main = NumberSumAlgorithmHelpers.GenerateNumbersWithTargetSum(
                maxRange: config.MainNumbersRange,
                count: config.MainNumbersCount,
                targetSum: targetSum,
                rng: rng)
            .ToImmutableArray();

        // 3) bonus numbers (random & distinct from main)
        var bonus = config.BonusNumbersCount > 0
            ? NumberSumAlgorithmHelpers.RandomDistinct(1, config.BonusNumbersRange, main, config.BonusNumbersCount, rng)
            : ImmutableArray<int>.Empty;

        // 4) confidence: inverse of average deviation from historical sums
        var confidence = NumberSumAlgorithmHelpers.CalculateNumberSumConfidence(history, main);

        return new PredictionResult(
            config.LotteryId,
            main,
            bonus,
            confidence,
            PredictionAlgorithmKeys.NumberSum);
    }

    // ---------- helpers (PURE) ----------
}