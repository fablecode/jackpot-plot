using System.Collections.Immutable;
using JackpotPlot.Domain.Models;

namespace JackpotPlot.Domain.Predictions.Helpers;

public sealed class SeasonalPatternsAlgorithmHelpers
{
    public static string GetSeason(DateTime dateUtc)
    {
        var m = dateUtc.Month;
        return m switch
        {
            12 or 1 or 2 => "Winter",
            3 or 4 or 5 => "Spring",
            6 or 7 or 8 => "Summer",
            9 or 10 or 11 => "Fall",
            _ => "Unknown"
        };
    }

    public static Dictionary<int, int> AnalyzeSeasonalFrequencies(
        IEnumerable<HistoricalDraw> historicalDraws,
        string season,
        int numberRange)
    {
        var freq = Enumerable.Range(1, numberRange).ToDictionary(n => n, _ => 0);

        foreach (var draw in historicalDraws)
        {
            if (GetSeason(draw.DrawDate) != season) continue;
            foreach (var n in draw.WinningNumbers)
                if (n >= 1 && n <= numberRange) freq[n]++;
        }

        return freq;
    }

    public static ImmutableArray<int> GenerateFromSeasonalFrequencies(
        Dictionary<int, int> seasonalFreq,
        int take,
        Random rng)
    {
        if (take <= 0) return ImmutableArray<int>.Empty;

        return seasonalFreq
            .OrderByDescending(kv => kv.Value)
            .ThenBy(_ => rng.Next())      // tie-breaker for equal frequencies
            .Select(kv => kv.Key)
            .Take(take)
            .ToImmutableArray();
    }

    public static double SeasonalConfidence(
        IEnumerable<HistoricalDraw> historicalDraws,
        List<int> predictedNumbers,
        string season)
    {
        var seasonal = historicalDraws.Where(d => GetSeason(d.DrawDate) == season).ToList();
        if (seasonal.Count == 0 || predictedNumbers.Count == 0) return 0d;

        var matches = 0;
        foreach (var d in seasonal)
            matches += d.WinningNumbers.Intersect(predictedNumbers).Count();

        return (double)matches / (seasonal.Count * predictedNumbers.Count);
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