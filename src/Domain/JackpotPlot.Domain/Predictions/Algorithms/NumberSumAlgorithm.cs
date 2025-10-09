using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.ValueObjects;
using System.Collections.Immutable;
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
        var targetSum = CalculateTargetSum(history);

        // 2) generate main numbers near the target sum
        var main = GenerateNumbersWithTargetSum(
                maxRange: config.MainNumbersRange,
                count: config.MainNumbersCount,
                targetSum: targetSum,
                rng: rng)
            .ToImmutableArray();

        // 3) bonus numbers (random & distinct from main)
        var bonus = config.BonusNumbersCount > 0
            ? RandomDistinct(1, config.BonusNumbersRange, main, config.BonusNumbersCount, rng)
            : ImmutableArray<int>.Empty;

        // 4) confidence: inverse of average deviation from historical sums
        var confidence = CalculateNumberSumConfidence(history, main);

        return new PredictionResult(
            config.LotteryId,
            main,
            bonus,
            confidence,
            PredictionAlgorithmKeys.NumberSum);
    }

    // ---------- helpers (PURE) ----------

    private static double CalculateTargetSum(IReadOnlyCollection<HistoricalDraw> draws)
        => draws.Select(d => d.WinningNumbers.Sum()).Average();

    private static List<int> GenerateNumbersWithTargetSum(
        int maxRange,
        int count,
        double targetSum,
        Random rng)
    {
        // Greedy-ish approach that nudges toward remaining average
        var numbers = new HashSet<int>();
        while (numbers.Count < count)
        {
            var remainingCount = count - numbers.Count;
            var remainingSum = targetSum - numbers.Sum();
            var avgNeeded = remainingCount == 0 ? 0 : remainingSum / remainingCount;

            // jitter around the needed average; clamp to valid range
            var jitter = rng.Next(-3, 4); // [-3, +3]
            var candidate = (int)Math.Round(avgNeeded + jitter, MidpointRounding.AwayFromZero);
            candidate = Math.Clamp(candidate, 1, maxRange);

            // avoid duplicates; if collision, try a couple random fallbacks
            if (!numbers.Add(candidate))
            {
                var tries = 0;
                while (tries++ < 5)
                {
                    candidate = rng.Next(1, maxRange + 1);
                    if (numbers.Add(candidate)) break;
                }
            }
        }

        // light shuffle for variety
        return numbers.OrderBy(_ => rng.Next()).ToList();
    }

    private static double CalculateNumberSumConfidence(
        IReadOnlyCollection<HistoricalDraw> draws,
        ImmutableArray<int> predicted)
    {
        if (predicted.Length == 0 || draws.Count == 0) return 0;

        var predictedSum = predicted.Sum();
        var avgDeviation = draws
            .Select(d => Math.Abs(d.WinningNumbers.Sum() - predictedSum))
            .Average();

        return 1.0 / (1.0 + avgDeviation);
    }

    private static ImmutableArray<int> RandomDistinct(
        int minInclusive,
        int maxInclusive,
        ImmutableArray<int> exclude,
        int count,
        Random rng)
    {
        if (count <= 0) return ImmutableArray<int>.Empty;

        var candidates = Enumerable.Range(minInclusive, maxInclusive - minInclusive + 1)
            .Except(exclude);
        return candidates
            .OrderBy(_ => rng.Next())
            .Take(count)
            .ToImmutableArray();
    }
}