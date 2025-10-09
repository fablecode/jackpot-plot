using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.ValueObjects;
using System.Collections.Immutable;
using JackpotPlot.Primitives.Algorithms;

namespace JackpotPlot.Domain.Predictions.Algorithms;

[PredictionAlgorithmDescription(PredictionAlgorithmKeys.SkewnessAnalysis, "sUses the skewness of the distribution of historical draw numbers to determine if the draw is biased toward higher or lower numbers, and predicts accordingly to match that trend.")]
public sealed class SkewnessAnalysisAlgorithm : IPredictionAlgorithm
{
    public PredictionResult Predict(
        LotteryConfigurationDomain config,
        IReadOnlyList<HistoricalDraw> history,
        Random rng)
    {
        if (history.Count == 0)
        {
            return new PredictionResult(
                config.LotteryId,
                ImmutableArray<int>.Empty,
                ImmutableArray<int>.Empty,
                0d,
                PredictionAlgorithmKeys.SkewnessAnalysis);
        }

        // 1) Analyze skewness from all historical main numbers
        var skewness = CalculateSkewness(history);

        // 2) Generate main numbers based on skewness signal
        var main = GenerateNumbersBasedOnSkewness(
                maxRange: config.MainNumbersRange,
                count: config.MainNumbersCount,
                skewness: skewness,
                rng: rng)
            .ToImmutableArray();

        // 3) Bonus numbers (random, distinct from main)
        var bonus = config.BonusNumbersCount > 0
            ? RandomDistinct(1, config.BonusNumbersRange, main, config.BonusNumbersCount, rng)
            : ImmutableArray<int>.Empty;

        // 4) Confidence (same spirit as original: closer mean + larger |skew| ↓ confidence)
        var confidence = CalculateSkewnessConfidence(history, main);

        return new PredictionResult(
            config.LotteryId,
            main,
            bonus,
            confidence,
            PredictionAlgorithmKeys.SkewnessAnalysis);
    }

    // ---------- helpers (PURE) ----------

    private static double CalculateSkewness(IReadOnlyCollection<HistoricalDraw> draws)
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

    private static ImmutableArray<int> GenerateNumbersBasedOnSkewness(
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

    private static double CalculateSkewnessConfidence(
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