using System.Collections.Immutable;
using JackpotPlot.Domain.Models;

namespace JackpotPlot.Domain.Predictions.Helpers;

public sealed class StandardDeviationAlgorithmHelpers
{
    public static double CalculateHistoricalStandardDeviation(IEnumerable<HistoricalDraw> draws)
    {
        var all = draws.SelectMany(d => d.WinningNumbers).Select(n => (double)n).ToList();
        if (all.Count == 0) return 0;

        var mean = all.Average();
        var variance = all.Sum(x => Math.Pow(x - mean, 2)) / all.Count;
        return Math.Sqrt(variance);
    }

    public static List<int> GenerateNumbersWithTargetStdDev(
        int maxRange,
        int count,
        double targetStdDev,
        Random rng)
    {
        if (count <= 0) return new();

        var numbers = new List<int>();

        while (numbers.Count < count)
        {
            var candidate = rng.Next(1, maxRange + 1);

            var candidateSet = numbers.Concat(new[] { candidate }).ToList();
            double currentStdDev = CalculateStandardDeviation(candidateSet);

            // accept if close enough to target or list empty
            if (numbers.Count == 0 || Math.Abs(currentStdDev - targetStdDev) < 0.5)
                numbers.Add(candidate);

            numbers = numbers.Distinct().ToList();
        }

        // shuffle for variety
        return numbers.OrderBy(_ => rng.Next()).Take(count).ToList();
    }

    private static double CalculateStandardDeviation(List<int> numbers)
    {
        if (numbers.Count == 0) return 0;
        var mean = numbers.Average();
        var variance = numbers.Sum(n => Math.Pow(n - mean, 2)) / numbers.Count;
        return Math.Sqrt(variance);
    }

    public static double CalculateStandardDeviationConfidence(
        IEnumerable<HistoricalDraw> draws,
        List<int> predicted)
    {
        if (!draws.Any() || predicted.Count == 0) return 0d;

        var histStdDev = CalculateHistoricalStandardDeviation(draws);
        var predStdDev = CalculateStandardDeviation(predicted);

        // confidence: 1 / (1 + difference)
        return 1.0 / (1.0 + Math.Abs(histStdDev - predStdDev));
    }

    public static ImmutableArray<int> RandomDistinct(
        int minInclusive,
        int maxInclusive,
        ImmutableArray<int> exclude,
        int count,
        Random rng)
    {
        if (count <= 0) return ImmutableArray<int>.Empty;

        var pool = Enumerable.Range(minInclusive, maxInclusive - minInclusive + 1)
            .Except(exclude);
        return pool.OrderBy(_ => rng.Next())
            .Take(count)
            .ToImmutableArray();
    }
}