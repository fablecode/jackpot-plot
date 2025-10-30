using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.ValueObjects;
using System.Collections.Immutable;
using JackpotPlot.Domain.Predictions.Helpers;
using JackpotPlot.Primitives.Algorithms;

namespace JackpotPlot.Domain.Predictions.Algorithms;

[PredictionAlgorithmDescription(PredictionAlgorithmKeys.CyclicPatterns, "Identifies recurring cycles or intervals in historical draws and predicts numbers that are due to appear based on these cyclic patterns.")]
public sealed class CyclicPatternsAlgorithm : IPredictionAlgorithm
{
    public PredictionResult Predict(
        LotteryConfigurationDomain config,
        IReadOnlyList<HistoricalDraw> history,
        Random rng)
    {
        if (history.Count == 0)
            return new PredictionResult(
                config.LotteryId, ImmutableArray<int>.Empty, ImmutableArray<int>.Empty, 0,
                PredictionAlgorithmKeys.CyclicPatterns);

        // 1) analyze cycles (distance between appearances for each number in range)
        var cycles = CyclicPatternsAlgorithmHelpers.AnalyzeCyclicPatterns(history, config.MainNumbersRange);

        // 2) choose main numbers by shortest average cycle first
        var main = CyclicPatternsAlgorithmHelpers.GenerateNumbersFromCycles(cycles, config.MainNumbersCount, rng).ToImmutableArray();

        // 3) bonus numbers (random, distinct from main)
        var bonus = config.BonusNumbersCount > 0
            ? CyclicPatternsAlgorithmHelpers.GenerateRandomNumbers(1, config.BonusNumbersRange, main, config.BonusNumbersCount, rng)
            : ImmutableArray<int>.Empty;

        // 4) naive confidence: fraction of predicted numbers that are "due" per their average cycle
        var confidence = CyclicPatternsAlgorithmHelpers.CalculateCyclicConfidence(history, main, cycles);

        return new PredictionResult(
            config.LotteryId,
            main,
            bonus,
            confidence,
            PredictionAlgorithmKeys.CyclicPatterns);
    }
}