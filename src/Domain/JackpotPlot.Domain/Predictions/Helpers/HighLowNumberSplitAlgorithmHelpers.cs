using System.Collections.Immutable;
using JackpotPlot.Domain.Models;

namespace JackpotPlot.Domain.Predictions.Helpers;

public static class HighLowNumberSplitAlgorithmHelpers
{
    public static (double lowRatio, double highRatio) AnalyzeHighLowSplit(
        IEnumerable<HistoricalDraw> historicalDraws, int numberRange)
    {
        int low = 0, high = 0;
        var mid = numberRange / 2;

        foreach (var draw in historicalDraws)
        {
            foreach (var n in draw.WinningNumbers)
            {
                if (n <= mid) low++;
                else high++;
            }
        }

        var total = low + high;
        return total == 0 ? (0.5, 0.5) : (low / (double)total, high / (double)total);
    }

    public static double DistributionConfidence(
        IEnumerable<HistoricalDraw> historicalDraws,
        ImmutableArray<int> predicted,
        int numberRange)
    {
        if (predicted.IsDefaultOrEmpty) return 0;

        var (histLowRatio, histHighRatio) = AnalyzeHighLowSplit(historicalDraws, numberRange);
        var mid = numberRange / 2;
        var predLow = predicted.Count(n => n <= mid);
        var predHigh = predicted.Length - predLow;

        var predLowRatio = predLow / (double)predicted.Length;
        var predHighRatio = predHigh / (double)predicted.Length;

        // L1 distance between distributions → map to (0..1], higher is more similar
        var distance = Math.Abs(predLowRatio - histLowRatio) + Math.Abs(predHighRatio - histHighRatio);
        return 1.0 / (1.0 + distance);
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