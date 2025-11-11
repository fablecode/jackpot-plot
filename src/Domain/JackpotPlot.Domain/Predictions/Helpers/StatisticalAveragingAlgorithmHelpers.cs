using System.Collections.Immutable;
using JackpotPlot.Domain.Models;

namespace JackpotPlot.Domain.Predictions.Helpers;

public sealed class StatisticalAveragingAlgorithmHelpers
{
    public static List<int> CalculateAverages(
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

    public static double CalculateStatisticalAveragingConfidence(
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