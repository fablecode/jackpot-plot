using System.Collections.Immutable;
using JackpotPlot.Domain.Models;

namespace JackpotPlot.Domain.Predictions.Helpers;

public sealed class WeightDistributionAlgorithmHelpers
{
    public static Dictionary<int, double> CalculateWeights(
        IReadOnlyCollection<HistoricalDraw> historicalDraws, int numberRange)
    {
        var weights = Enumerable.Range(1, numberRange)
            .ToDictionary(n => n, _ => 0.0);

        foreach (var draw in historicalDraws)
        foreach (var n in draw.WinningNumbers)
            if (n >= 1 && n <= numberRange) weights[n]++;

        var total = weights.Values.Sum();
        if (total <= 0)
        {
            // uniform fallback
            var p = 1.0 / numberRange;
            foreach (var k in weights.Keys.ToList()) weights[k] = p;
            return weights;
        }

        foreach (var k in weights.Keys.ToList())
            weights[k] /= total;

        return weights;
    }

    public static ImmutableArray<int> GenerateWeightedRandomNumbers(
        Dictionary<int, double> weights, int count, Random rng)
    {
        if (count <= 0) return ImmutableArray<int>.Empty;

        // sample without replacement by re-normalizing after each pick
        var remaining = new Dictionary<int, double>(weights);
        var picked = new List<int>(capacity: count);

        while (picked.Count < count && remaining.Count > 0)
        {
            var total = remaining.Values.Sum();
            if (total <= 0)
            {
                // fallback to uniform among remaining
                var pickU = remaining.Keys.OrderBy(_ => rng.Next()).First();
                picked.Add(pickU);
                remaining.Remove(pickU);
                continue;
            }

            var roll = rng.NextDouble() * total;
            double acc = 0;
            foreach (var (n, w) in remaining)
            {
                acc += w;
                if (roll <= acc)
                {
                    picked.Add(n);
                    remaining.Remove(n);
                    break;
                }
            }
        }

        return picked.OrderBy(_ => rng.Next()).ToImmutableArray();
    }

    public static double CalculateWeightDistributionConfidence(
        IReadOnlyCollection<HistoricalDraw> historicalDraws, List<int> predicted)
    {
        if (historicalDraws.Count == 0 || predicted.Count == 0) return 0d;

        var matches = 0;
        foreach (var d in historicalDraws)
            matches += d.WinningNumbers.Intersect(predicted).Count();

        return (double)matches / (historicalDraws.Count * predicted.Count);
    }

    public static ImmutableArray<int> RandomDistinct(
        int minInclusive, int maxInclusive,
        ImmutableArray<int> exclude, int count, Random rng)
    {
        if (count <= 0) return ImmutableArray<int>.Empty;

        var candidates = Enumerable.Range(minInclusive, maxInclusive - minInclusive + 1)
            .Except(exclude);
        return candidates.OrderBy(_ => rng.Next())
            .Take(count)
            .ToImmutableArray();
    }
}