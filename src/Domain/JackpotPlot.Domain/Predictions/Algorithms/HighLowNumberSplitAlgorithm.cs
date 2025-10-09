using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.ValueObjects;
using System.Collections.Immutable;
using JackpotPlot.Primitives.Algorithms;

namespace JackpotPlot.Domain.Predictions.Algorithms;

[PredictionAlgorithmDescription(PredictionAlgorithmKeys.HighLowNumberSplit, "Divides the number range into low and high segments and generates predictions by balancing the selection between these two groups according to historical trends.")]
public sealed class HighLowNumberSplitAlgorithm : IPredictionAlgorithm
{
    public PredictionResult Predict(
        LotteryConfigurationDomain config,
        IReadOnlyList<HistoricalDraw> history,
        Random rng)
    {
        // No history → fallback to even split + random
        if (history.Count == 0)
        {
            var evenLow = config.MainNumbersCount / 2;
            var evenHigh = config.MainNumbersCount - evenLow;

            var low = RandomDistinct(1, config.MainNumbersRange / 2, ImmutableArray<int>.Empty, evenLow, rng);
            var high = RandomDistinct(config.MainNumbersRange / 2 + 1, config.MainNumbersRange, low, evenHigh, rng);

            var mainFallback = low.Concat(high).OrderBy(_ => rng.Next()).ToImmutableArray();
            var bonusFallback = config.BonusNumbersCount > 0
                ? RandomDistinct(1, config.BonusNumbersRange, mainFallback, config.BonusNumbersCount, rng)
                : ImmutableArray<int>.Empty;

            return new PredictionResult(
                config.LotteryId, mainFallback, bonusFallback, 0, PredictionAlgorithmKeys.HighLowNumberSplit);
        }

        // 1) Analyze historical low/high ratios
        var (lowRatio, highRatio) = AnalyzeHighLowSplit(history, config.MainNumbersRange);

        // 2) Determine counts for each half
        var lowCount = (int)Math.Round(config.MainNumbersCount * lowRatio, MidpointRounding.AwayFromZero);
        lowCount = Math.Clamp(lowCount, 0, config.MainNumbersCount);
        var highCount = config.MainNumbersCount - lowCount;

        // 3) Sample from halves (distinct)
        var lowNums = RandomDistinct(1, config.MainNumbersRange / 2, ImmutableArray<int>.Empty, lowCount, rng);
        var highNums = RandomDistinct(config.MainNumbersRange / 2 + 1, config.MainNumbersRange, lowNums, highCount, rng);

        var main = lowNums.Concat(highNums).OrderBy(_ => rng.Next()).ToImmutableArray();

        // 4) Bonus numbers (random, distinct from main)
        var bonus = config.BonusNumbersCount > 0
            ? RandomDistinct(1, config.BonusNumbersRange, main, config.BonusNumbersCount, rng)
            : ImmutableArray<int>.Empty;

        // 5) Confidence (simple distribution similarity score)
        var confidence = DistributionConfidence(history, main, config.MainNumbersRange);

        return new PredictionResult(
            config.LotteryId,
            main,
            bonus,
            confidence,
            PredictionAlgorithmKeys.HighLowNumberSplit);
    }

    // ---------- helpers (PURE) ----------

    private static (double lowRatio, double highRatio) AnalyzeHighLowSplit(
        IEnumerable<HistoricalDraw> historicalDraws, int numberRange)
    {
        int low = 0, high = 0;
        int mid = numberRange / 2;

        foreach (var draw in historicalDraws)
        {
            foreach (var n in draw.WinningNumbers)
            {
                if (n <= mid) low++;
                else high++;
            }
        }

        var total = low + high;
        if (total == 0) return (0.5, 0.5);
        return (low / (double)total, high / (double)total);
    }

    private static double DistributionConfidence(
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