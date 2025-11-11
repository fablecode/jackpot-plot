using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.ValueObjects;
using System.Collections.Immutable;
using JackpotPlot.Domain.Predictions.Helpers;
using JackpotPlot.Primitives.Algorithms;

namespace JackpotPlot.Domain.Predictions.Algorithms;

[PredictionAlgorithmDescription(PredictionAlgorithmKeys.StatisticalAveraging, "Computes averages (mean/median) of historical draws to predict numbers that tend to cluster around these average values.")]
public sealed class StatisticalAveragingAlgorithm : IPredictionAlgorithm
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
                0d,
                PredictionAlgorithmKeys.StatisticalAveraging);
        }

        // 1) per-position means for main numbers
        var mainAverages = StatisticalAveragingAlgorithmHelpers.CalculateAverages(
            history,
            config.MainNumbersCount,
            config.MainNumbersRange,
            isBonus: false).ToImmutableArray();

        // 2) per-position means for bonus numbers (if applicable)
        var bonusAverages = config.BonusNumbersCount > 0
            ? StatisticalAveragingAlgorithmHelpers.CalculateAverages(history,
                config.BonusNumbersCount,
                config.BonusNumbersRange,
                isBonus: true).ToImmutableArray()
            : ImmutableArray<int>.Empty;

        // 3) confidence = inverse average deviation vs historical per-draw average
        var confidence = StatisticalAveragingAlgorithmHelpers.CalculateStatisticalAveragingConfidence(history, mainAverages);

        return new PredictionResult(
            config.LotteryId,
            mainAverages,
            bonusAverages,
            confidence,
            PredictionAlgorithmKeys.StatisticalAveraging);
    }
}