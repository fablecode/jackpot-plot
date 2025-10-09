using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.ValueObjects;
using System.Collections.Immutable;
using JackpotPlot.Primitives.Algorithms;

namespace JackpotPlot.Domain.Predictions.Algorithms;

[PredictionAlgorithmDescription(PredictionAlgorithmKeys.InvertedFrequency, "Emphasizes “cold” numbers that have appeared less frequently in the past, under the theory that these may be more likely to be drawn in the future.")]
public sealed class InvertedFrequencyAlgorithm : IPredictionAlgorithm
{
    public PredictionResult Predict(
        LotteryConfigurationDomain config,
        IReadOnlyList<HistoricalDraw> history,
        Random rng)
    {
        // 1) frequency map over the main-number range
        var freq = AnalyzeInvertedFrequencies(history, config.MainNumbersRange);

        // 2) choose the least-frequent main numbers (break ties with rng)
        var main = GenerateFromInvertedFrequencies(freq, config.MainNumbersCount, rng)
            .ToImmutableArray();

        // 3) bonus numbers (random, distinct from main)
        var bonus = config.BonusNumbersCount > 0
            ? RandomDistinct(1, config.BonusNumbersRange, main, config.BonusNumbersCount, rng)
            : ImmutableArray<int>.Empty;

        // 4) confidence = historical overlap rate (same idea as original)
        var confidence = InvertedFrequencyConfidence(history, main.ToList());

        return new PredictionResult(
            config.LotteryId,
            main,
            bonus,
            confidence,
            PredictionAlgorithmKeys.InvertedFrequency);
    }

    // ---------- helpers (PURE) ----------

    private static Dictionary<int, int> AnalyzeInvertedFrequencies(
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

    private static IEnumerable<int> GenerateFromInvertedFrequencies(
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

    private static double InvertedFrequencyConfidence(
        IReadOnlyList<HistoricalDraw> draws,
        List<int> predicted)
    {
        if (draws.Count == 0 || predicted.Count == 0) return 0d;

        var matches = 0;
        foreach (var d in draws)
            matches += d.WinningNumbers.Intersect(predicted).Count();

        return (double)matches / (draws.Count * predicted.Count);
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
        return candidates.OrderBy(_ => rng.Next())
            .Take(count)
            .ToImmutableArray();
    }
}