using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.ValueObjects;
using System.Collections.Immutable;
using JackpotPlot.Domain.Predictions.Helpers;
using JackpotPlot.Primitives.Algorithms;

namespace JackpotPlot.Domain.Predictions.Algorithms;

[PredictionAlgorithmDescription(PredictionAlgorithmKeys.DeltaSystem, "Examines the differences (deltas) between consecutive numbers in past draws and uses these patterns to generate predictions.")]
public sealed class DeltaSystemAlgorithm : IPredictionAlgorithm
{
    public PredictionResult Predict(
        LotteryConfigurationDomain config,
        IReadOnlyList<HistoricalDraw> history,
        Random rng)
    {
        // 1) collect all deltas from history
        var allDeltas = DeltaSystemAlgorithmHelpers.CalculateDeltas(history);

        // if we’ve got no history, just random-fill
        if (allDeltas.Count == 0)
        {
            var fallback = DeltaSystemAlgorithmHelpers.RandomDistinct(1, config.MainNumbersRange, [], config.MainNumbersCount, rng);
            var bonusFb = config.BonusNumbersCount > 0
                ? DeltaSystemAlgorithmHelpers.RandomDistinct(1, config.BonusNumbersRange, fallback, config.BonusNumbersCount, rng)
                : ImmutableArray<int>.Empty;

            return new PredictionResult(
                config.LotteryId, fallback, bonusFb, 0, PredictionAlgorithmKeys.DeltaSystem);
        }

        // 2) pick most frequent deltas; for a line of N numbers we need up to N-1 deltas
        var neededDeltas = Math.Max(0, config.MainNumbersCount - 1);
        var frequentDeltas = DeltaSystemAlgorithmHelpers.GetFrequentDeltas(allDeltas, neededDeltas);

        // 3) generate main numbers from the deltas (start in lower half; respect range)
        var main = DeltaSystemAlgorithmHelpers.GenerateNumbersFromDeltas(
            frequentDeltas, config.MainNumbersRange, config.MainNumbersCount, rng);

        // 4) bonus numbers (random, distinct from main)
        var bonus = config.BonusNumbersCount > 0
            ? DeltaSystemAlgorithmHelpers.RandomDistinct(1, config.BonusNumbersRange, main, config.BonusNumbersCount, rng)
            : ImmutableArray<int>.Empty;

        // 5) confidence (how many predicted deltas match typical historical deltas)
        var confidence = DeltaSystemAlgorithmHelpers.CalculateDeltaSystemConfidence(history, main.ToList());

        return new PredictionResult(
            config.LotteryId, main, bonus, confidence, PredictionAlgorithmKeys.DeltaSystem);
    }
}