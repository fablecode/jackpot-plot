using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.ValueObjects;
using System.Collections.Immutable;
using JackpotPlot.Domain.Predictions.Helpers;
using JackpotPlot.Primitives.Algorithms;

namespace JackpotPlot.Domain.Predictions.Algorithms;

[PredictionAlgorithmDescription(PredictionAlgorithmKeys.RarePatterns, "Identifies unusual or infrequent patterns in historical draws and uses these “rare” patterns as the basis for predictions, assuming they might be due for an occurrence.")]
public sealed class RarePatternsAlgorithm : IPredictionAlgorithm
{
    public PredictionResult Predict(
        LotteryConfigurationDomain config,
        IReadOnlyList<HistoricalDraw> history,
        Random rng)
    {
        // No history → empty (or fallback to random if you prefer)
        if (history.Count == 0)
        {
            return new PredictionResult(
                config.LotteryId,
                ImmutableArray<int>.Empty,
                ImmutableArray<int>.Empty,
                0d,
                PredictionAlgorithmKeys.RarePatterns);
        }

        // 1) analyze patterns (e.g., "2L3H-3O2E")
        var rarePatterns = RarePatternsAlgorithmHelpers.AnalyzeRarePatterns(history, config.MainNumbersRange);

        // 2) generate from the rarest pattern
        var main = RarePatternsAlgorithmHelpers.GenerateFromRarestPattern(
                rarePatterns,
                count: config.MainNumbersCount,
                numberRange: config.MainNumbersRange,
                rng)
            .ToImmutableArray();

        // 3) bonus (random & distinct from main)
        var bonus = config.BonusNumbersCount > 0
            ? RarePatternsAlgorithmHelpers.RandomDistinct(1, config.BonusNumbersRange, main, config.BonusNumbersCount, rng)
            : ImmutableArray<int>.Empty;

        // 4) confidence: higher for rarer patterns (same spirit as original)
        var confidence = RarePatternsAlgorithmHelpers.CalculateRarePatternsConfidence(history, main.ToList(), rarePatterns, config.MainNumbersRange);

        return new PredictionResult(
            config.LotteryId,
            main,
            bonus,
            confidence,
            PredictionAlgorithmKeys.RarePatterns);
    }
}