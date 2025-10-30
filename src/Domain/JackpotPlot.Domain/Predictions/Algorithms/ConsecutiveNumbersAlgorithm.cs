using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.ValueObjects;
using System.Collections.Immutable;
using JackpotPlot.Domain.Predictions.Helpers;
using JackpotPlot.Primitives.Algorithms;

namespace JackpotPlot.Domain.Predictions.Algorithms;

[PredictionAlgorithmDescription(PredictionAlgorithmKeys.ConsecutiveNumbers, "Focuses on frequently occurring consecutive pairs or sequences, assuming that numbers appearing in a chain might appear together again.")]
public sealed class ConsecutiveNumbersAlgorithm : IPredictionAlgorithm
{
    public PredictionResult Predict(LotteryConfigurationDomain config, IReadOnlyList<HistoricalDraw> history, Random rng)
    {
        var consecutivePairs = ConsecutiveNumbersAlgorithmHelpers.FindFrequentConsecutivePairs(history);
        var selected = ConsecutiveNumbersAlgorithmHelpers.SelectConsecutiveNumbers(consecutivePairs, config.MainNumbersCount);

        var remaining = ConsecutiveNumbersAlgorithmHelpers.GenerateRandomNumbers(
            1, config.MainNumbersRange, selected, config.MainNumbersCount - selected.Count, rng);

        var predicted = selected.Concat(remaining).OrderBy(_ => rng.Next()).ToImmutableArray();

        var bonus = config.BonusNumbersCount > 0
            ? ConsecutiveNumbersAlgorithmHelpers.GenerateRandomNumbers(1, config.BonusNumbersRange, new List<int>(), config.BonusNumbersCount, rng)
            : ImmutableArray<int>.Empty;

        return new PredictionResult(
            config.LotteryId,
            predicted,
            bonus,
            ConsecutiveNumbersAlgorithmHelpers.CalculateConsecutiveNumbersConfidence(history, predicted.ToList()),
            PredictionAlgorithmKeys.ConsecutiveNumbers);
    }
}