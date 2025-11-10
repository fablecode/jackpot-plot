using System.Collections.Immutable;
using JackpotPlot.Domain.Models;

namespace JackpotPlot.Domain.Predictions.Helpers;

public sealed class RarePatternsAlgorithmHelpers
{
    public static Dictionary<string, int> AnalyzeRarePatterns(
        IEnumerable<HistoricalDraw> historicalDraws,
        int numberRange)
    {
        var patternFrequency = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var draw in historicalDraws)
        {
            var pattern = BuildPattern(draw.WinningNumbers, numberRange);
            if (!patternFrequency.TryAdd(pattern, 1))
                patternFrequency[pattern]++;
        }
        // ascending by frequency = rarest first
        return patternFrequency
            .OrderBy(kv => kv.Value)
            .ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.Ordinal);
    }

    private static string BuildPattern(IReadOnlyList<int> numbers, int numberRange)
    {
        var mid = numberRange / 2;
        var low = numbers.Count(n => n <= mid);
        var high = numbers.Count - low;

        var odd = numbers.Count(n => (n & 1) == 1);
        var even = numbers.Count - odd;

        // e.g., "2L3H-3O2E"
        return $"{low}L{high}H-{odd}O{even}E";
    }

    public static ImmutableArray<int> GenerateFromRarestPattern(
        Dictionary<string, int> rarePatterns,
        int count,
        int numberRange,
        Random rng)
    {
        if (count <= 0 || rarePatterns.Count == 0) return ImmutableArray<int>.Empty;

        var rarest = rarePatterns.Keys.First(); // already sorted rare→common
        var parts = rarest.Split('-', StringSplitOptions.RemoveEmptyEntries);
        var (lowCount, highCount) = ParseLowHigh(parts[0]);
        var (oddCount, evenCount) = ParseOddEven(parts[1]);

        // sample low/high pools first
        var low = Enumerable.ToList(RandomDistinct(1, numberRange / 2, ImmutableArray<int>.Empty, lowCount, rng));
        var high = Enumerable.ToList(RandomDistinct(numberRange / 2 + 1, numberRange, low.ToImmutableArray(), highCount, rng));

        var pool = low.Concat(high).ToList();

        // balance odd/even within the pool
        var odds = Enumerable.Take(pool.Where(n => (n & 1) == 1), oddCount);
        var evens = Enumerable.Take(pool.Where(n => (n & 1) == 0), evenCount);
        var selected = odds.Concat(evens).Distinct().ToList();

        // Fill if needed (distinct)
        if (selected.Count < count)
        {
            var fill = RandomDistinct(1, numberRange, selected.ToImmutableArray(), count - selected.Count, rng);
            selected.AddRange(fill);
        }

        return selected
            .OrderBy(_ => rng.Next())
            .Take(count)
            .ToImmutableArray();
    }

    private static (int low, int high) ParseLowHigh(string token)
    {
        // token like "2L3H"
        var parts = token.Split(['L', 'H'], StringSplitOptions.RemoveEmptyEntries);
        var low = int.Parse(parts[0]);
        var high = int.Parse(parts[1]);
        return (low, high);
    }

    private static (int odd, int even) ParseOddEven(string token)
    {
        // token like "3O2E"
        var parts = token.Split(['O', 'E'], StringSplitOptions.RemoveEmptyEntries);
        var odd = int.Parse(parts[0]);
        var even = int.Parse(parts[1]);
        return (odd, even);
    }

    public static double CalculateRarePatternsConfidence(
        IEnumerable<HistoricalDraw> historicalDraws,
        List<int> predictedNumbers,
        Dictionary<string, int> rarePatterns,
        int numberRange)
    {
        if (predictedNumbers.Count == 0 || rarePatterns.Count == 0) return 0d;

        var predictedPattern = BuildPattern(predictedNumbers, numberRange);
        // “rarer → higher confidence”: 1 / (1 + frequency)
        return rarePatterns.TryGetValue(predictedPattern, out var freq) ? 1.0 / (1.0 + freq) : 1.0;
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