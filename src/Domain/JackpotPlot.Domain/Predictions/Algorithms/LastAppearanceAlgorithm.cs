using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.ValueObjects;
using System.Collections.Immutable;
using JackpotPlot.Primitives.Algorithms;

namespace JackpotPlot.Domain.Predictions.Algorithms;

[PredictionAlgorithmDescription(PredictionAlgorithmKeys.LastAppearance, "Prioritizes numbers that have not appeared for a long time, considering them “overdue” and likely to be drawn in the next draw.")]
public sealed class LastAppearanceAlgorithm : IPredictionAlgorithm
{
    public PredictionResult Predict(LotteryConfigurationDomain config, IReadOnlyList<HistoricalDraw> history, Random rng)
    {
        var last = TrackLastAppearances(history.ToList(), config.MainNumbersRange);

        var overdue = last.OrderByDescending(kv => kv.Value)
            .ThenBy(kv => kv.Key)
            .Take(config.MainNumbersCount)
            .Select(kv => kv.Key)
            .ToImmutableArray();

        var bonus = config.BonusNumbersCount > 0
            ? GenerateRandom(1, config.BonusNumbersRange, new List<int>(), config.BonusNumbersCount, rng)
            : ImmutableArray<int>.Empty;

        return new PredictionResult(
            config.LotteryId,
            overdue,
            bonus,
            LastAppearanceConfidence(history, overdue.ToList()),
            PredictionAlgorithmKeys.LastAppearance);
    }

    private static Dictionary<int, int> TrackLastAppearances(IList<HistoricalDraw> draws, int range)
    {
        var last = Enumerable.Range(1, range).ToDictionary(n => n, _ => int.MaxValue);
        for (int i = draws.Count - 1; i >= 0; i--)
            foreach (var n in draws[i].WinningNumbers)
                if (last[n] == int.MaxValue) last[n] = draws.Count - i;
        return last;
    }

    private static double LastAppearanceConfidence(IEnumerable<HistoricalDraw> history, List<int> predicted)
    {
        var total = history.Count();
        if (total == 0 || predicted.Count == 0) return 0;
        var matches = history.Sum(d => d.WinningNumbers.Intersect(predicted).Count());
        return (double)matches / (total * predicted.Count);
    }

    private static ImmutableArray<int> GenerateRandom(int min, int max, List<int> exclude, int count, Random rng)
        => Enumerable.Range(min, max - min + 1).Except(exclude).OrderBy(_ => rng.Next()).Take(count).ToImmutableArray();
}