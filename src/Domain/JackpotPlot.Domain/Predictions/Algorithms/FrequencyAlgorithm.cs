using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.ValueObjects;
using System.Collections.Immutable;
using JackpotPlot.Domain.Predictions.Helpers;
using JackpotPlot.Primitives.Algorithms;

namespace JackpotPlot.Domain.Predictions.Algorithms;

[PredictionAlgorithmDescription(PredictionAlgorithmKeys.FrequencyBased, "Selects numbers based on their historical frequency—identifying “hot” numbers that appear most often in previous draws.")]
public sealed class FrequencyAlgorithm : IPredictionAlgorithm
{
    public PredictionResult Predict(LotteryConfigurationDomain config, IReadOnlyList<HistoricalDraw> history, Random rng)
    {
        var mainFreq = FrequencyAlgorithmHelpers.CalcFreq(history.SelectMany(d => d.WinningNumbers), config.MainNumbersRange);
        var bonusFreq = config.BonusNumbersCount > 0
            ? FrequencyAlgorithmHelpers.CalcFreq(history.SelectMany(d => d.BonusNumbers), config.BonusNumbersRange)
            : new List<(int Number, int Frequency)>();

        var main = mainFreq.OrderByDescending(f => f.Frequency)
            .Take(config.MainNumbersCount)
            .Select(f => f.Number)
            .ToImmutableArray();

        var bonus = bonusFreq.OrderByDescending(f => f.Frequency)
            .Take(config.BonusNumbersCount)
            .Select(f => f.Number)
            .ToImmutableArray();

        return new PredictionResult(
            config.LotteryId,
            main,
            bonus, FrequencyAlgorithmHelpers.FrequencyConfidence(history, main.ToList()),
            PredictionAlgorithmKeys.FrequencyBased);
    }
}