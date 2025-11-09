using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.ValueObjects;
using System.Collections.Immutable;
using JackpotPlot.Domain.Predictions.Helpers;
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
        var freq = InvertedFrequencyAlgorithmHelpers.AnalyzeInvertedFrequencies(history, config.MainNumbersRange);

        // 2) choose the least-frequent main numbers (break ties with rng)
        var main = InvertedFrequencyAlgorithmHelpers.GenerateFromInvertedFrequencies(freq, config.MainNumbersCount, rng)
            .ToImmutableArray();

        // 3) bonus numbers (random, distinct from main)
        var bonus = config.BonusNumbersCount > 0
            ? InvertedFrequencyAlgorithmHelpers.RandomDistinct(1, config.BonusNumbersRange, main, config.BonusNumbersCount, rng)
            : ImmutableArray<int>.Empty;

        // 4) confidence = historical overlap rate (same idea as original)
        var confidence = InvertedFrequencyAlgorithmHelpers.InvertedFrequencyConfidence(history, main.ToList());

        return new PredictionResult(
            config.LotteryId,
            main,
            bonus,
            confidence,
            PredictionAlgorithmKeys.InvertedFrequency);
    }
}