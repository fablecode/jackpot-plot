using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.ValueObjects;
using System.Collections.Immutable;
using JackpotPlot.Domain.Predictions.Helpers;
using JackpotPlot.Primitives.Algorithms;

namespace JackpotPlot.Domain.Predictions.Algorithms;

[PredictionAlgorithmDescription(PredictionAlgorithmKeys.GroupSelection, "Splits the number range into predefined groups (e.g., low, medium, high) and selects numbers proportionally from each group based on historical frequency.")]
public sealed class GroupSelectionAlgorithm : IPredictionAlgorithm
{
    public PredictionResult Predict(
        LotteryConfigurationDomain config,
        IReadOnlyList<HistoricalDraw> history,
        Random rng)
    {
        // 1) make 3 groups (low/medium/high). tweak groupCount if you want more buckets.
        const int groupCount = 3;
        var groups = GroupSelectionAlgorithmHelpers.DivideIntoGroups(config.MainNumbersRange, groupCount);

        // 2) build frequency per group from history
        var groupFreq = GroupSelectionAlgorithmHelpers.AnalyzeGroupFrequencies(history, groups);

        // 3) pick main numbers proportionally to group frequency
        var main = GroupSelectionAlgorithmHelpers.GenerateNumbersFromGroups(groups, groupFreq, config.MainNumbersCount, rng)
            .ToImmutableArray();

        // 4) make bonus numbers (random, distinct from main)
        var bonus = config.BonusNumbersCount > 0
            ? GroupSelectionAlgorithmHelpers.RandomDistinct(1, config.BonusNumbersRange, main, config.BonusNumbersCount, rng)
            : ImmutableArray<int>.Empty;

        // 5) confidence: compare predicted group distribution to historical distribution
        var confidence = GroupSelectionAlgorithmHelpers.CalculateGroupConfidence(history, main, groups, groupFreq);

        return new PredictionResult(
            config.LotteryId,
            main,
            bonus,
            confidence,
            PredictionAlgorithmKeys.GroupSelection);
    }
}