using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.ValueObjects;
using System.Collections.Immutable;
using JackpotPlot.Primitives.Algorithms;

namespace JackpotPlot.Domain.Predictions.Algorithms;

[PredictionAlgorithmDescription(PredictionAlgorithmKeys.FrequencyBased, "Selects numbers based on their historical frequency—identifying “hot” numbers that appear most often in previous draws.")]
public sealed class FrequencyAlgorithm : IPredictionAlgorithm
{
    public PredictionResult Predict(LotteryConfigurationDomain config, IReadOnlyList<HistoricalDraw> history, Random rng)
    {
        var mainFreq = CalcFreq(history.SelectMany(d => d.WinningNumbers), config.MainNumbersRange);
        var bonusFreq = config.BonusNumbersCount > 0
            ? CalcFreq(history.SelectMany(d => d.BonusNumbers), config.BonusNumbersRange)
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
            bonus,
            FrequencyConfidence(history, main.ToList()),
            PredictionAlgorithmKeys.FrequencyBased);
    }

    private static List<(int Number, int Frequency)> CalcFreq(IEnumerable<int> nums, int range)
    {
        var freq = new int[range + 1];
        foreach (var n in nums) freq[n]++;
        return Enumerable.Range(1, range).Select(n => (n, freq[n])).ToList();
    }

    private static double FrequencyConfidence(IEnumerable<HistoricalDraw> history, List<int> predicted)
    {
        var total = history.Count();
        if (total == 0 || predicted.Count == 0) return 0;
        var matches = history.Sum(d => d.WinningNumbers.Intersect(predicted).Count());
        return (double)matches / (total * predicted.Count);
    }
}