using System.Collections.Immutable;
using JackpotPlot.Domain.Models;

namespace JackpotPlot.Domain.Predictions.Helpers;

public sealed class SkewnessAnalysisAlgorithmHelpers
{
    public static double CalculateSkewness(IReadOnlyCollection<HistoricalDraw> draws)
    {
        var all = draws.SelectMany(d => d.WinningNumbers).Select(n => (double)n).ToList();
        if (all.Count < 2) return 0;

        var mean = all.Average();
        var variance = all.Sum(x => Math.Pow(x - mean, 2)) / all.Count;
        var std = Math.Sqrt(Math.Max(variance, double.Epsilon));

        var m3 = all.Sum(x => Math.Pow(x - mean, 3)) / all.Count;
        var skew = m3 / Math.Pow(std, 3);

        return double.IsFinite(skew) ? skew : 0;
    }

    public static ImmutableArray<int> GenerateNumbersBasedOnSkewness(
        int maxRange,
        int count,
        double skewness,
        Random rng)
    {
        if (count <= 0) return ImmutableArray<int>.Empty;

        // Favor low half if skew > 0, favor high half if skew < 0, neutral if ~0.
        // Use a soft probability so we still explore across the range.
        var half = maxRange / 2;
        var targetLowBias = skewness > 0.10 ? 0.75 : (Math.Abs(skewness) <= 0.10 ? 0.50 : 0.25);
        var targetHighBias = 1.0 - targetLowBias;

        var picked = new HashSet<int>();
        while (picked.Count < count)
        {
            // Choose which half to sample from according to bias.
            var pickLow = rng.NextDouble() < targetLowBias;
            int min = pickLow ? 1 : (half + 1);
            int max = pickLow ? half : maxRange;

            // If half is degenerate (e.g., odd ranges), fall back to full range.
            if (min > max) { min = 1; max = maxRange; }

            var candidate = rng.Next(min, max + 1);
            if (picked.Add(candidate)) { /* added */ }

            // If we got stuck (very small halves), top-up from full range
            if (picked.Count < count && picked.Count % 5 == 0)
            {
                var remaining = Enumerable.Range(1, maxRange).Except(picked)
                    .OrderBy(_ => rng.Next())
                    .Take(Math.Max(0, count - picked.Count));
                foreach (var n in remaining) picked.Add(n);
            }
        }

        // light shuffle for variety
        return picked.OrderBy(_ => rng.Next()).Take(count).ToImmutableArray();
    }

    public static double CalculateSkewnessConfidence(
        IReadOnlyCollection<HistoricalDraw> draws,
        ImmutableArray<int> predicted)
    {
        if (draws.Count == 0 || predicted.IsDefaultOrEmpty) return 0d;

        var actualAll = draws.SelectMany(d => d.WinningNumbers).Select(n => (double)n).ToList();
        var actualMean = actualAll.Average();
        var actualSkew = CalculateSkewness(draws);

        var predictedMean = predicted.Average();

        // Same form as original idea: 1 / (1 + |mean error| + |skew|)
        var score = 1.0 / (1.0 + Math.Abs(predictedMean - actualMean) + Math.Abs(actualSkew));
        return score;
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
        return candidates.OrderBy(_ => rng.Next())
            .Take(count)
            .ToImmutableArray();
    }
}