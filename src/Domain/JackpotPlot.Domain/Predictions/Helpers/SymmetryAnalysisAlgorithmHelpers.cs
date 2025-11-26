using System.Collections.Immutable;
using JackpotPlot.Domain.Models;

namespace JackpotPlot.Domain.Predictions.Helpers;

public sealed class SymmetryAnalysisAlgorithmHelpers
{
    public static (double highLowRatio, double oddEvenRatio) AnalyzeSymmetryMetrics(
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

    public static ImmutableArray<int> GenerateSymmetricNumbers(
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

    public static double CalculateSymmetryConfidence(
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

    public static ImmutableArray<int> RandomDistinct(
        int minInclusive, int maxInclusive,
        ImmutableArray<int> exclude, int count, Random rng)
    {
        if (count <= 0) return ImmutableArray<int>.Empty;
        var candidates = Enumerable.Range(minInclusive, maxInclusive - minInclusive + 1)
            .Except(exclude);
        return candidates.OrderBy(_ => rng.Next()).Take(count).ToImmutableArray();
    }
}