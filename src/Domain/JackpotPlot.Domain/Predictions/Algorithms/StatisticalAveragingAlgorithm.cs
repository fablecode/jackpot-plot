using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.ValueObjects;
using System.Collections.Immutable;
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
        var mainAverages = CalculateAverages(
            history,
            config.MainNumbersCount,
            config.MainNumbersRange,
            isBonus: false).ToImmutableArray();

        // 2) per-position means for bonus numbers (if applicable)
        var bonusAverages = config.BonusNumbersCount > 0
            ? CalculateAverages(history,
                config.BonusNumbersCount,
                config.BonusNumbersRange,
                isBonus: true).ToImmutableArray()
            : ImmutableArray<int>.Empty;

        // 3) confidence = inverse average deviation vs historical per-draw average
        var confidence = CalculateStatisticalAveragingConfidence(history, mainAverages);

        return new PredictionResult(
            config.LotteryId,
            mainAverages,
            bonusAverages,
            confidence,
            PredictionAlgorithmKeys.StatisticalAveraging);
    }

    // ---------- helpers (PURE) ----------

    private static List<int> CalculateAverages(
        IReadOnlyList<HistoricalDraw> historicalDraws,
        int numbersCount,
        int maxRange,
        bool isBonus)
    {
        var averages = new List<int>(capacity: numbersCount);

        // For each position, take the mean of available values and clamp into range.
        for (int pos = 0; pos < numbersCount; pos++)
        {
            var atPosition = historicalDraws
                .Select(d => isBonus
                    ? d.BonusNumbers.ElementAtOrDefault(pos)
                    : d.WinningNumbers.ElementAtOrDefault(pos))
                .Where(n => n > 0)
                .ToList();

            if (atPosition.Count > 0)
            {
                var mean = Math.Round(atPosition.Average(), MidpointRounding.AwayFromZero);
                averages.Add((int)Math.Clamp(mean, 1, maxRange));
            }
        }

        // If some positions were missing entirely, backfill by sampling near global mean.
        if (averages.Count < numbersCount)
        {
            var global = (int)Math.Round(
                (isBonus
                    ? historicalDraws.SelectMany(d => d.BonusNumbers)
                    : historicalDraws.SelectMany(d => d.WinningNumbers)).DefaultIfEmpty(0).Average(),
                MidpointRounding.AwayFromZero);

            while (averages.Count < numbersCount)
            {
                // jitter around global mean to avoid duplicates
                var jitter = Math.Clamp(global + (averages.Count % 2 == 0 ? 1 : -1), 1, maxRange);
                averages.Add(jitter);
            }
        }

        return averages;
    }

    private static double CalculateStatisticalAveragingConfidence(
        IReadOnlyList<HistoricalDraw> history,
        ImmutableArray<int> predictedMain)
    {
        if (predictedMain.IsDefaultOrEmpty || history.Count == 0) return 0d;

        var predictedAvg = predictedMain.Average();
        var perDrawAverages = history.Select(d => d.WinningNumbers.Average()).ToList();

        var avgDeviation = perDrawAverages.Select(a => Math.Abs(a - predictedAvg)).Average();
        return 1.0 / (1.0 + avgDeviation);
    }
}