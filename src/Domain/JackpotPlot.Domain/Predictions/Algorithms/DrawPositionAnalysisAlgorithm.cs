using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.ValueObjects;
using System.Collections.Immutable;
using JackpotPlot.Domain.Predictions.Helpers;
using JackpotPlot.Primitives.Algorithms;

namespace JackpotPlot.Domain.Predictions.Algorithms;

[PredictionAlgorithmDescription(PredictionAlgorithmKeys.DrawPositionAnalysis, "Evaluates the positions in which numbers appear (first, second, etc.) in historical draws to predict numbers that are likely to occur in those same positions.")]
public sealed class DrawPositionAnalysisAlgorithm : IPredictionAlgorithm
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
                0,
                PredictionAlgorithmKeys.DrawPositionAnalysis);
        }

        // 1) compute per-position frequency tables for main numbers
        var positionFrequencies = DrawPositionAnalysisAlgorithmHelpers.AnalyzeDrawPositions(
            history, config.MainNumbersCount, config.MainNumbersRange);

        // 2) choose one number per position (break ties with rng), then shuffle a bit
        var main = DrawPositionAnalysisAlgorithmHelpers.GenerateNumbersFromPositions(positionFrequencies, rng)
            .Take(config.MainNumbersCount)
            .ToImmutableArray();

        // 3) bonus numbers (random & distinct from main)
        var bonus = config.BonusNumbersCount > 0
            ? DrawPositionAnalysisAlgorithmHelpers.GenerateRandomNumbers(1, config.BonusNumbersRange, main, config.BonusNumbersCount, rng)
            : ImmutableArray<int>.Empty;

        // 4) simple confidence: fraction of exact position matches historically
        var confidence = DrawPositionAnalysisAlgorithmHelpers.CalculatePositionConfidence(history, main);

        return new PredictionResult(
            config.LotteryId,
            main,
            bonus,
            confidence,
            PredictionAlgorithmKeys.DrawPositionAnalysis);
    }
}