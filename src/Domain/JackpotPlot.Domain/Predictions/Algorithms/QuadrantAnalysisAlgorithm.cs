using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.ValueObjects;
using System.Collections.Immutable;
using JackpotPlot.Domain.Predictions.Helpers;
using JackpotPlot.Primitives.Algorithms;

namespace JackpotPlot.Domain.Predictions.Algorithms;

[PredictionAlgorithmDescription(PredictionAlgorithmKeys.QuadrantAnalysis, "Divides the number range into quadrants and selects numbers based on the frequency distribution within each quadrant observed in historical data.")]
public sealed class QuadrantAnalysisAlgorithm : IPredictionAlgorithm
{
    public PredictionResult Predict(
        LotteryConfigurationDomain config,
        IReadOnlyList<HistoricalDraw> history,
        Random rng)
    {
        // No history → random fill
        if (history.Count == 0)
        {
            var mainEmpty = ImmutableArray<int>.Empty;
            var bonusEmpty = ImmutableArray<int>.Empty;

            return new PredictionResult(
                config.LotteryId, mainEmpty, bonusEmpty, 0d, PredictionAlgorithmKeys.QuadrantAnalysis);
        }

        // 1) build quadrants over the main-number range
        var quadrants = QuadrantAnalysisAlgorithmHelpers.DivideIntoQuadrants(config.MainNumbersRange, 4);

        // 2) analyze quadrant frequencies from history
        var quadFreq = QuadrantAnalysisAlgorithmHelpers.AnalyzeQuadrantFrequencies(history, quadrants);

        // 3) generate main numbers proportional to quadrant frequency
        var main = QuadrantAnalysisAlgorithmHelpers.GenerateNumbersFromQuadrants(
                quadrants, quadFreq, config.MainNumbersCount, rng)
            .ToImmutableArray();

        // 4) bonus numbers (random, distinct from main)
        var bonus = config.BonusNumbersCount > 0
            ? QuadrantAnalysisAlgorithmHelpers.RandomDistinct(1, config.BonusNumbersRange, main, config.BonusNumbersCount, rng)
            : ImmutableArray<int>.Empty;

        // 5) confidence: distribution similarity (predicted vs historical)
        var confidence = QuadrantAnalysisAlgorithmHelpers.CalculateQuadrantConfidence(history, main.ToList(), quadrants);

        return new PredictionResult(
            config.LotteryId,
            main,
            bonus,
            confidence,
            PredictionAlgorithmKeys.QuadrantAnalysis);
    }
}