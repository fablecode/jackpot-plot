using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.ValueObjects;
using System.Collections.Immutable;
using JackpotPlot.Domain.Predictions.Helpers;
using JackpotPlot.Primitives.Algorithms;

namespace JackpotPlot.Domain.Predictions.Algorithms;

[PredictionAlgorithmDescription(PredictionAlgorithmKeys.PatternMatching, "Identifies recurring patterns (such as odd/even or high/low sequences) in historical data and uses these patterns as templates for future predictions.")]
public sealed class PatternMatchingAlgorithm : IPredictionAlgorithm
{
    public PredictionResult Predict(
        LotteryConfigurationDomain config,
        IReadOnlyList<HistoricalDraw> history,
        Random rng)
    {
        // No history → return empty (or you could random-fill if you prefer)
        if (history.Count == 0)
        {
            return new PredictionResult(
                config.LotteryId,
                ImmutableArray<int>.Empty,
                ImmutableArray<int>.Empty,
                0d,
                PredictionAlgorithmKeys.PatternMatching);
        }

        // 1) Build pattern frequencies from history (uses O/E + H/L like original)
        var patterns = PatternMatchingAlgorithmHelpers.AnalyzePatterns(history, config);

        // 2) Select most frequent pattern (fallback: empty pattern -> random)
        var selectedPattern = PatternMatchingAlgorithmHelpers.SelectMostFrequentPattern(patterns);

        // 3) Generate main numbers matching the pattern tokens
        ImmutableArray<int> main;
        if (!string.IsNullOrEmpty(selectedPattern))
        {
            main = PatternMatchingAlgorithmHelpers.GenerateNumbersFromPattern(selectedPattern, config.MainNumbersRange, rng);
        }
        else
        {
            main = PatternMatchingAlgorithmHelpers.RandomDistinct(1, config.MainNumbersRange, ImmutableArray<int>.Empty, config.MainNumbersCount, rng);
        }

        // 4) Bonus numbers (random, distinct from main)
        var bonus = config.BonusNumbersCount > 0
            ? PatternMatchingAlgorithmHelpers.RandomDistinct(1, config.BonusNumbersRange, main, config.BonusNumbersCount, rng)
            : ImmutableArray<int>.Empty;

        // 5) Confidence: share of historical draws whose full pattern equals predicted pattern
        var confidence = PatternMatchingAlgorithmHelpers.CalculatePatternMatchingConfidence(history, selectedPattern, config);

        return new PredictionResult(
            config.LotteryId,
            main,
            bonus,
            confidence,
            PredictionAlgorithmKeys.PatternMatching);
    }
}