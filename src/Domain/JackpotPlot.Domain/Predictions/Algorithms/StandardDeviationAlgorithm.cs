using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.ValueObjects;
using System.Collections.Immutable;
using JackpotPlot.Domain.Predictions.Helpers;
using JackpotPlot.Primitives.Algorithms;

namespace JackpotPlot.Domain.Predictions.Algorithms;

[PredictionAlgorithmDescription(PredictionAlgorithmKeys.StandardDeviation, "Uses the spread (standard deviation) of historical draws to generate predictions that mimic the overall variability in the numbers.")]
public sealed class StandardDeviationAlgorithm : IPredictionAlgorithm
{
    public PredictionResult Predict(
        LotteryConfigurationDomain config,
        IReadOnlyList<HistoricalDraw> history,
        Random rng)
    {
        // no history → empty
        if (history.Count == 0)
        {
            return new PredictionResult(
                config.LotteryId,
                ImmutableArray<int>.Empty,
                ImmutableArray<int>.Empty,
                0d,
                PredictionAlgorithmKeys.StandardDeviation);
        }

        // 1) calculate historical std deviation
        var targetStdDev = StandardDeviationAlgorithmHelpers.CalculateHistoricalStandardDeviation(history);

        // 2) generate numbers approximating that std dev
        var main = StandardDeviationAlgorithmHelpers.GenerateNumbersWithTargetStdDev(
                config.MainNumbersRange,
                config.MainNumbersCount,
                targetStdDev,
                rng)
            .ToImmutableArray();

        // 3) bonus numbers (random distinct)
        var bonus = config.BonusNumbersCount > 0
            ? StandardDeviationAlgorithmHelpers.RandomDistinct(1, config.BonusNumbersRange, main, config.BonusNumbersCount, rng)
            : ImmutableArray<int>.Empty;

        // 4) confidence = closeness of predicted vs historical std dev
        var confidence = StandardDeviationAlgorithmHelpers.CalculateStandardDeviationConfidence(history, main.ToList());

        return new PredictionResult(
            config.LotteryId,
            main,
            bonus,
            confidence,
            PredictionAlgorithmKeys.StandardDeviation);
    }
}