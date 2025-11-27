using System.Collections.Immutable;
using JackpotPlot.Domain.Models;

namespace JackpotPlot.Domain.Predictions.Helpers;

public sealed class TimeDecayAlgorithmHelpers
{
    public static Dictionary<int, double> AssignTimeDecayWeights(
        IEnumerable<HistoricalDraw> draws, double decay)
    {
        var map = new Dictionary<int, double>();
        int idx = 0;
        foreach (var d in draws.OrderByDescending(d => d.DrawDate))
            map[d.DrawId] = Math.Pow(decay, idx++);
        return map;
    }

    public static Dictionary<int, double> CalculateWeightedFrequencies(
        IEnumerable<HistoricalDraw> draws,
        Dictionary<int, double> weightsById,
        int range)
    {
        var freq = Enumerable.Range(1, range).ToDictionary(n => n, _ => 0.0);
        foreach (var d in draws)
        {
            var w = weightsById[d.DrawId];
            foreach (var n in d.WinningNumbers)
                if (n >= 1 && n <= range) freq[n] += w;
        }
        return freq;
    }

    public static IEnumerable<int> WeightedSampleDistinct(
        Dictionary<int, double> weights,
        int take,
        Random rng)
    {
        if (take <= 0) yield break;

        // normalize to a CDF over the remaining items each pick
        var remaining = new Dictionary<int, double>(weights);
        for (int i = 0; i < take && remaining.Count > 0; i++)
        {
            var total = remaining.Values.Sum();
            if (total <= 0)
            {
                // fallback to uniform if all weights zero
                var pickU = remaining.Keys.OrderBy(_ => rng.Next()).First();
                yield return pickU;
                remaining.Remove(pickU);
                continue;
            }

            var roll = rng.NextDouble() * total;
            double cum = 0;
            foreach (var (n, w) in remaining)
            {
                cum += w;
                if (roll <= cum)
                {
                    yield return n;
                    remaining.Remove(n);
                    break;
                }
            }
        }
    }

    public static double TimeDecayConfidence(IReadOnlyList<HistoricalDraw> history, List<int> predicted)
    {
        if (predicted.Count == 0) return 0d;

        var recent = history.OrderByDescending(d => d.DrawDate).Take(10).ToList();
        if (recent.Count == 0) return 0d;

        var matches = recent.Sum(d => d.WinningNumbers.Intersect(predicted).Count());
        return (double)matches / (recent.Count * predicted.Count);
    }

    public static ImmutableArray<int> RandomDistinct(
        int minInclusive, int maxInclusive,
        ImmutableArray<int> exclude, int count, Random rng)
    {
        if (count <= 0) return ImmutableArray<int>.Empty;
        var candidates = Enumerable.Range(minInclusive, maxInclusive - minInclusive + 1)
            .Except(exclude);
        return candidates.OrderBy(_ => rng.Next()).Take(count).ToImmutableArray();
    }
}