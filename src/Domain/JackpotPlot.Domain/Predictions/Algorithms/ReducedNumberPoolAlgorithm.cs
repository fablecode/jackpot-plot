using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.ValueObjects;
using System.Collections.Immutable;
using JackpotPlot.Primitives.Algorithms;

namespace JackpotPlot.Domain.Predictions.Algorithms;

[PredictionAlgorithmDescription(PredictionAlgorithmKeys.ReducedNumberPool, "Narrows the candidate pool by excluding numbers that appear infrequently, thereby focusing predictions on historically more likely numbers.")]
public sealed class ReducedNumberPoolAlgorithm : IPredictionAlgorithm
{
    private readonly double _appearanceThresholdRatio;

    /// <param name="appearanceThresholdRatio">
    /// Ratio (0..1] of draws a number must appear in to remain in the pool.
    /// Default is 0.10 (10%), matching the original strategy.
    /// </param>
    public ReducedNumberPoolAlgorithm(double appearanceThresholdRatio = 0.10)
    {
        _appearanceThresholdRatio = Math.Clamp(appearanceThresholdRatio, 0, 1);
    }

    public PredictionResult Predict(
        LotteryConfigurationDomain config,
        IReadOnlyList<HistoricalDraw> history,
        Random rng)
    {
        // 1) Build reduced pool from history
        var reducedPool = AnalyzeReducedNumberPool(history, config.MainNumbersRange, _appearanceThresholdRatio);

        // 2) Generate main numbers (fill from full range if pool is too small)
        var main = GenerateFromReducedPool(
                reducedPool, config.MainNumbersRange, config.MainNumbersCount, rng)
            .ToImmutableArray();

        // 3) Bonus numbers (random, distinct from main)
        var bonus = config.BonusNumbersCount > 0
            ? RandomDistinct(1, config.BonusNumbersRange, main, config.BonusNumbersCount, rng)
            : ImmutableArray<int>.Empty;

        // 4) Confidence: historical overlap rate with predicted numbers
        var confidence = CalculateReducedPoolConfidence(history, main.ToList());

        return new PredictionResult(
            config.LotteryId,
            main,
            bonus,
            confidence,
            PredictionAlgorithmKeys.ReducedNumberPool);
    }

    // ---------- helpers (PURE) ----------

    private static List<int> AnalyzeReducedNumberPool(
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

    private static IEnumerable<int> GenerateFromReducedPool(
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

    private static double CalculateReducedPoolConfidence(
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
        return candidates
            .OrderBy(_ => rng.Next())
            .Take(count)
            .ToImmutableArray();
    }
}