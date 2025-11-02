using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.ValueObjects;
using System.Collections.Immutable;
using JackpotPlot.Domain.Predictions.Helpers;
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

            var low = HighLowNumberSplitAlgorithmHelpers.RandomDistinct(1, config.MainNumbersRange / 2, ImmutableArray<int>.Empty, evenLow, rng);
            var high = HighLowNumberSplitAlgorithmHelpers.RandomDistinct(config.MainNumbersRange / 2 + 1, config.MainNumbersRange, low, evenHigh, rng);

            var mainFallback = low.Concat(high).OrderBy(_ => rng.Next()).ToImmutableArray();
            var bonusFallback = config.BonusNumbersCount > 0
                ? HighLowNumberSplitAlgorithmHelpers.RandomDistinct(1, config.BonusNumbersRange, mainFallback, config.BonusNumbersCount, rng)
                : ImmutableArray<int>.Empty;

            return new PredictionResult(
                config.LotteryId, mainFallback, bonusFallback, 0, PredictionAlgorithmKeys.HighLowNumberSplit);
        }

        // 1) Analyze historical low/high ratios
        var (lowRatio, highRatio) = HighLowNumberSplitAlgorithmHelpers.AnalyzeHighLowSplit(history, config.MainNumbersRange);

        // 2) Determine counts for each half
        var lowCount = (int)Math.Round(config.MainNumbersCount * lowRatio, MidpointRounding.AwayFromZero);
        lowCount = Math.Clamp(lowCount, 0, config.MainNumbersCount);
        var highCount = config.MainNumbersCount - lowCount;

        // 3) Sample from halves (distinct)
        var lowNums = HighLowNumberSplitAlgorithmHelpers.RandomDistinct(1, config.MainNumbersRange / 2, ImmutableArray<int>.Empty, lowCount, rng);
        var highNums = HighLowNumberSplitAlgorithmHelpers.RandomDistinct(config.MainNumbersRange / 2 + 1, config.MainNumbersRange, lowNums, highCount, rng);

        var main = lowNums.Concat(highNums).OrderBy(_ => rng.Next()).ToImmutableArray();

        // 4) Bonus numbers (random, distinct from main)
        var bonus = config.BonusNumbersCount > 0
            ? HighLowNumberSplitAlgorithmHelpers.RandomDistinct(1, config.BonusNumbersRange, main, config.BonusNumbersCount, rng)
            : ImmutableArray<int>.Empty;

        // 5) Confidence (simple distribution similarity score)
        var confidence = HighLowNumberSplitAlgorithmHelpers.DistributionConfidence(history, main, config.MainNumbersRange);

        return new PredictionResult(
            config.LotteryId,
            main,
            bonus,
            confidence,
            PredictionAlgorithmKeys.HighLowNumberSplit);
    }
}