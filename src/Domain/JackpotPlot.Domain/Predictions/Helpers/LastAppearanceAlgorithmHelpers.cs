using System.Collections.Immutable;
using JackpotPlot.Domain.Models;

namespace JackpotPlot.Domain.Predictions.Helpers;

public sealed class LastAppearanceAlgorithmHelpers
{
    public static Dictionary<int, int> TrackLastAppearances(IList<HistoricalDraw> draws, int range)
    {
        var last = Enumerable.Range(1, range).ToDictionary(n => n, _ => int.MaxValue);
        for (var i = draws.Count - 1; i >= 0; i--)
        {
            foreach (var n in draws[i].WinningNumbers)
            {
                if (last[n] == int.MaxValue) last[n] = draws.Count - i;
            }
        }

        return last;
    }

    public static double LastAppearanceConfidence(IEnumerable<HistoricalDraw> history, List<int> predicted)
    {
        var historicalDraws = history as HistoricalDraw[] ?? history.ToArray();
        var total = historicalDraws.Count();
        if (total == 0 || predicted.Count == 0)
        {
            return 0;
        }

        var matches = historicalDraws.Sum(d => d.WinningNumbers.Intersect(predicted).Count());
        return (double)matches / (total * predicted.Count);
    }

    public static ImmutableArray<int> GenerateRandom(int min, int max, List<int> exclude, int count, Random rng)
        => Enumerable.Range(min, max - min + 1).Except(exclude).OrderBy(_ => rng.Next()).Take(count).ToImmutableArray();
}