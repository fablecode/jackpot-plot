using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.ValueObjects;
using System.Collections.Immutable;
using JackpotPlot.Domain.Predictions.Helpers;
using JackpotPlot.Primitives.Algorithms;

namespace JackpotPlot.Domain.Predictions.Algorithms;

[PredictionAlgorithmDescription(PredictionAlgorithmKeys.LastAppearance, "Prioritizes numbers that have not appeared for a long time, considering them “overdue” and likely to be drawn in the next draw.")]
public sealed class LastAppearanceAlgorithm : IPredictionAlgorithm
{
    public PredictionResult Predict(LotteryConfigurationDomain config, IReadOnlyList<HistoricalDraw> history, Random rng)
    {
        var last = LastAppearanceAlgorithmHelpers.TrackLastAppearances(history.ToList(), config.MainNumbersRange);

        var overdue = last.OrderByDescending(kv => kv.Value)
            .ThenBy(kv => kv.Key)
            .Take(config.MainNumbersCount)
            .Select(kv => kv.Key)
            .ToImmutableArray();

        var bonus = config.BonusNumbersCount > 0
            ? LastAppearanceAlgorithmHelpers.GenerateRandom(1, config.BonusNumbersRange, new List<int>(), config.BonusNumbersCount, rng)
            : ImmutableArray<int>.Empty;

        return new PredictionResult(
            config.LotteryId,
            overdue,
            bonus, LastAppearanceAlgorithmHelpers.LastAppearanceConfidence(history, overdue.ToList()),
            PredictionAlgorithmKeys.LastAppearance);
    }
}