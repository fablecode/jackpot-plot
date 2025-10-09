using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.ValueObjects;
using System.Collections.Immutable;
using JackpotPlot.Primitives.Algorithms;

namespace JackpotPlot.Domain.Predictions.Algorithms;

[PredictionAlgorithmDescription(PredictionAlgorithmKeys.SymmetryAnalysis, "Evaluates the balance between high/low and odd/even numbers in historical draws to produce predictions that maintain a symmetric or balanced distribution reflective of past trends.")]
public sealed class SymmetryAnalysisAlgorithm : IPredictionAlgorithm
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
                PredictionAlgorithmKeys.SymmetryAnalysis);
        }

        // analyze historical symmetry (high/low + odd/even)
        var metrics = AnalyzeSymmetryMetrics(history, config.MainNumbersRange);

        // generate main numbers matching the symmetry ratios
        var main = GenerateSymmetricNumbers(metrics, config.MainNumbersCount, config.MainNumbersRange, rng)
            .ToImmutableArray();

        // bonus numbers (random & distinct from main)
        var bonus = config.BonusNumbersCount > 0
            ? RandomDistinct(1, config.BonusNumbersRange, main, config.BonusNumbersCount, rng)
            : ImmutableArray<int>.Empty;

        // confidence: how close our predicted ratios are to historical ratios
        var confidence = CalculateSymmetryConfidence(main, metrics, config.MainNumbersRange);

        return new PredictionResult(
            config.LotteryId, main, bonus, confidence, PredictionAlgorithmKeys.SymmetryAnalysis);
    }

    private static (double highLowRatio, double oddEvenRatio) AnalyzeSymmetryMetrics(
        IEnumerable<HistoricalDraw> draws, int numberRange)
    {
        int high = 0, low = 0, odd = 0, even = 0;
        int mid = numberRange / 2;

        foreach (var d in draws)
        {
            foreach (var n in d.WinningNumbers)
            {
                if (n > mid) high++; else low++;
                if ((n & 1) == 1) odd++; else even++;
            }
        }

        // avoid /0; if either side is 0, treat ratio as 0 (or 1 if both zero)
        double hl = (low == 0) ? (high == 0 ? 1d : double.PositiveInfinity) : (double)high / low;
        double oe = (even == 0) ? (odd == 0 ? 1d : double.PositiveInfinity) : (double)odd / even;

        // clamp infinities to a large finite value to keep calculations stable
        hl = double.IsFinite(hl) ? hl : 1e6;
        oe = double.IsFinite(oe) ? oe : 1e6;
        return (hl, oe);
    }

    private static ImmutableArray<int> GenerateSymmetricNumbers(
        (double highLowRatio, double oddEvenRatio) metrics,
        int count,
        int numberRange,
        Random rng)
    {
        if (count <= 0) return ImmutableArray<int>.Empty;

        int mid = numberRange / 2;

        // convert ratios r into proportion r/(1+r)
        int highTarget = (int)Math.Round(count * (metrics.highLowRatio / (1 + metrics.highLowRatio)), MidpointRounding.AwayFromZero);
        highTarget = Math.Clamp(highTarget, 0, count);
        int lowTarget = count - highTarget;

        int oddTarget = (int)Math.Round(count * (metrics.oddEvenRatio / (1 + metrics.oddEvenRatio)), MidpointRounding.AwayFromZero);
        oddTarget = Math.Clamp(oddTarget, 0, count);
        int evenTarget = count - oddTarget;

        // sample highs & lows
        var highs = Enumerable.Range(mid + 1, numberRange - mid).OrderBy(_ => rng.Next()).Take(highTarget).ToList();
        var lows = Enumerable.Range(1, mid).OrderBy(_ => rng.Next()).Take(lowTarget).ToList();
        var pool = highs.Concat(lows).ToList();

        // enforce odd/even proportions from the pool
        var odds = pool.Where(n => (n & 1) == 1).OrderBy(_ => rng.Next()).Take(oddTarget);
        var evens = pool.Where(n => (n & 1) == 0).OrderBy(_ => rng.Next()).Take(evenTarget);
        var selected = odds.Concat(evens).Distinct().ToList();

        // top-up if short
        if (selected.Count < count)
        {
            var fill = Enumerable.Range(1, numberRange)
                .Except(selected)
                .OrderBy(_ => rng.Next())
                .Take(count - selected.Count);
            selected.AddRange(fill);
        }

        return selected.OrderBy(_ => rng.Next()).Take(count).ToImmutableArray();
    }

    private static double CalculateSymmetryConfidence(
        ImmutableArray<int> predicted,
        (double highLowRatio, double oddEvenRatio) metrics,
        int numberRange)
    {
        if (predicted.IsDefaultOrEmpty) return 0d;

        int mid = numberRange / 2;
        int high = predicted.Count(n => n > mid);
        int low = predicted.Length - high;
        int odd = predicted.Count(n => (n & 1) == 1);
        int even = predicted.Length - odd;

        double predHL = (low == 0) ? (high == 0 ? 1d : 1e6) : (double)high / low;
        double predOE = (even == 0) ? (odd == 0 ? 1d : 1e6) : (double)odd / even;

        double hlErr = Math.Abs(predHL - metrics.highLowRatio);
        double oeErr = Math.Abs(predOE - metrics.oddEvenRatio);
        return 1.0 / (1.0 + hlErr + oeErr);
    }

    private static ImmutableArray<int> RandomDistinct(
        int minInclusive, int maxInclusive,
        ImmutableArray<int> exclude, int count, Random rng)
    {
        if (count <= 0) return ImmutableArray<int>.Empty;
        var candidates = Enumerable.Range(minInclusive, maxInclusive - minInclusive + 1)
            .Except(exclude);
        return candidates.OrderBy(_ => rng.Next()).Take(count).ToImmutableArray();
    }
}