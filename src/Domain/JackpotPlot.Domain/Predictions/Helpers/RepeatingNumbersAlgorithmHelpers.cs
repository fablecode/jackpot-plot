using System.Collections.Immutable;
using JackpotPlot.Domain.Models;

namespace JackpotPlot.Domain.Predictions.Helpers;

public sealed class RepeatingNumbersAlgorithmHelpers
{
    public static List<int> GetRecentNumbers(IReadOnlyList<HistoricalDraw> history, int take)
        => history.Take(take).SelectMany(d => d.WinningNumbers).ToList();

    public static Dictionary<int, int> IdentifyRepeatingNumbers(List<int> numbers)
    {
        var freq = new Dictionary<int, int>();
        foreach (var n in numbers)
            freq[n] = freq.GetValueOrDefault(n) + 1;

        return freq.Where(kv => kv.Value > 1)
            .OrderByDescending(kv => kv.Value)
            .ToDictionary(kv => kv.Key, kv => kv.Value);
    }

    public static List<int> GeneratePredictionsFromRepeatingNumbers(Dictionary<int, int> repeating, int count)
        => repeating.Keys.Take(count).ToList();

    public static double CalculateRepeatingNumbersConfidence(
        IReadOnlyCollection<HistoricalDraw> draws,
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