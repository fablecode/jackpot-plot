using System.Collections.Immutable;
using JackpotPlot.Domain.Models;

namespace JackpotPlot.Domain.Predictions.Helpers;

public sealed class NumberSumAlgorithmHelpers
{
    public static double CalculateTargetSum(IReadOnlyCollection<HistoricalDraw> draws)
        => draws.Select(d => d.WinningNumbers.Sum()).Average();

    public static List<int> GenerateNumbersWithTargetSum(
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

    public static double CalculateNumberSumConfidence(
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

    public static ImmutableArray<int> RandomDistinct(
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