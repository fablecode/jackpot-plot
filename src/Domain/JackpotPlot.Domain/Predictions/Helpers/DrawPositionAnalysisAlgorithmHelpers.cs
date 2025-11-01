using System.Collections.Immutable;
using JackpotPlot.Domain.Models;

namespace JackpotPlot.Domain.Predictions.Helpers;

public static class DrawPositionAnalysisAlgorithmHelpers
{
    public static Dictionary<int, Dictionary<int, int>> AnalyzeDrawPositions(
        IReadOnlyCollection<HistoricalDraw> draws,
        int mainNumbersCount,
        int numberRange)
    {
        var positionFrequencies = new Dictionary<int, Dictionary<int, int>>(mainNumbersCount);

        // init frequency tables
        for (var pos = 0; pos < mainNumbersCount; pos++)
        {
            var freq = new Dictionary<int, int>(numberRange);
            for (var n = 1; n <= numberRange; n++) freq[n] = 0;
            positionFrequencies[pos] = freq;
        }

        // accumulate
        foreach (var draw in draws)
        {
            for (var pos = 0; pos < mainNumbersCount && pos < draw.WinningNumbers.Count; pos++)
            {
                var number = draw.WinningNumbers[pos];
                positionFrequencies[pos][number]++;
            }
        }

        return positionFrequencies;
    }

    public static IEnumerable<int> GenerateNumbersFromPositions(
        Dictionary<int, Dictionary<int, int>> positionFrequencies,
        Random rng)
    {
        var selected = new List<int>();

        foreach (var pos in positionFrequencies.Keys.OrderBy(p => p))
        {
            // pick most frequent for this position; break ties with rng
            var pick = positionFrequencies[pos]
                .OrderByDescending(kv => kv.Value)
                .ThenBy(_ => rng.Next())
                .Select(kv => kv.Key)
                .First();

            if (!selected.Contains(pick))
                selected.Add(pick);
        }

        // small shuffle so output isn’t always monotonically increasing by position
        return selected.OrderBy(_ => rng.Next());
    }

    public static double CalculatePositionConfidence(
        IReadOnlyCollection<HistoricalDraw> draws,
        ImmutableArray<int> predicted)
    {
        if (draws.Count == 0 || predicted.Length == 0) return 0;

        var matches = 0;
        var totalPositions = 0;

        foreach (var d in draws)
        {
            var limit = Math.Min(predicted.Length, d.WinningNumbers.Count);
            totalPositions += limit;

            for (var pos = 0; pos < limit; pos++)
                if (d.WinningNumbers[pos] == predicted[pos]) matches++;
        }

        return totalPositions == 0 ? 0 : (double)matches / totalPositions;
    }

    public static ImmutableArray<int> GenerateRandomNumbers(
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