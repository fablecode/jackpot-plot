using System.Collections.Immutable;
using JackpotPlot.Domain.Models;

namespace JackpotPlot.Domain.Predictions.Helpers;

public sealed class ReducedNumberPoolAlgorithmHelpers
{
    public static List<int> AnalyzeReducedNumberPool(
        IReadOnlyList<HistoricalDraw> draws,
        int numberRange,
        double thresholdRatio)
    {
        if (draws.Count == 0)
            return Enumerable.Range(1, numberRange).ToList();

        var occ = new int[numberRange + 1];
        foreach (var d in draws)
        foreach (var n in d.WinningNumbers)
            if (n >= 1 && n <= numberRange) occ[n]++;

        var threshold = thresholdRatio * draws.Count; // e.g., 10% of draws
        var pool = new List<int>(numberRange);
        for (int n = 1; n <= numberRange; n++)
            if (occ[n] >= threshold) pool.Add(n);

        // if everything was filtered out, fall back to full range
        return pool.Count > 0 ? pool : Enumerable.Range(1, numberRange).ToList();
    }

    public static IEnumerable<int> GenerateFromReducedPool(
        List<int> reducedPool,
        int numberRange,
        int take,
        Random rng)
    {
        if (take <= 0) return Enumerable.Empty<int>();

        // sample from reduced pool first
        var selected = reducedPool.OrderBy(_ => rng.Next())
            .Take(take)
            .ToList();

        // if pool too small, top up from the full range excluding already selected
        if (selected.Count < take)
        {
            var fill = Enumerable.Range(1, numberRange)
                .Except(selected)
                .OrderBy(_ => rng.Next())
                .Take(take - selected.Count);
            selected.AddRange(fill);
        }

        // slight shuffle for variety
        return selected.OrderBy(_ => rng.Next()).Take(take);
    }

    public static double CalculateReducedPoolConfidence(
        IReadOnlyList<HistoricalDraw> draws,
        List<int> predicted)
    {
        if (draws.Count == 0 || predicted.Count == 0) return 0d;

        var matches = 0;
        foreach (var d in draws)
            matches += d.WinningNumbers.Intersect(predicted).Count();

        return (double)matches / (draws.Count * predicted.Count);
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