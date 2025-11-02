using System.Collections.Immutable;
using JackpotPlot.Domain.Models;

namespace JackpotPlot.Domain.Predictions.Helpers;

public sealed class InvertedFrequencyAlgorithmHelpers
{
    public static Dictionary<int, int> AnalyzeInvertedFrequencies(
        IReadOnlyList<HistoricalDraw> draws,
        int numberRange)
    {
        var map = Enumerable.Range(1, numberRange).ToDictionary(n => n, _ => 0);

        foreach (var d in draws)
        foreach (var n in d.WinningNumbers)
            if (n >= 1 && n <= numberRange) map[n]++;

        // ascending by frequency (coldest first)
        return map.OrderBy(kv => kv.Value)
            .ToDictionary(kv => kv.Key, kv => kv.Value);
    }

    public static IEnumerable<int> GenerateFromInvertedFrequencies(
        Dictionary<int, int> invertedFreqAsc,
        int take,
        Random rng)
    {
        if (take <= 0) return Enumerable.Empty<int>();

        // break ties with randomness while keeping low→high frequency priority
        return invertedFreqAsc
            .GroupBy(kv => kv.Value)                 // group by frequency
            .OrderBy(g => g.Key)                     // colder groups first
            .SelectMany(g => g.OrderBy(_ => rng.Next())
                .Select(kv => kv.Key))
            .Take(take);
    }

    public static double InvertedFrequencyConfidence(
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
        return candidates.OrderBy(_ => rng.Next())
            .Take(count)
            .ToImmutableArray();
    }
}