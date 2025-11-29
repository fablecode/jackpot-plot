using System.Collections.Immutable;
using JackpotPlot.Domain.Models;

namespace JackpotPlot.Domain.Predictions.Helpers;

public sealed class WeightedProbabilityAlgorithmHelpers
{
    public static Dictionary<int, double> CalculateWeights(
        IReadOnlyCollection<HistoricalDraw> historicalDraws,
        int numberRange)
    {
        var freq = new double[numberRange + 1];
        foreach (var d in historicalDraws)
        {
            foreach (var n in d.WinningNumbers)
                if (n >= 1 && n <= numberRange) freq[n]++;
        }

        var total = freq.Sum();
        var weights = new Dictionary<int, double>(capacity: numberRange);
        if (total <= 0)
        {
            // uniform fallback
            var p = 1.0 / numberRange;
            for (int n = 1; n <= numberRange; n++) weights[n] = p;
            return weights;
        }

        for (int n = 1; n <= numberRange; n++)
            weights[n] = freq[n] / total;

        return weights;
    }

    public static ImmutableArray<int> WeightedSampleDistinct(
        Dictionary<int, double> weights,
        int count,
        Random rng)
    {
        if (count <= 0) return ImmutableArray<int>.Empty;

        // copy (number → weight); keep ordered for stable traversal
        var remaining = weights.OrderBy(kv => kv.Key).ToDictionary(kv => kv.Key, kv => kv.Value);
        var picked = new List<int>(capacity: count);

        while (picked.Count < count && remaining.Count > 0)
        {
            var sum = remaining.Values.Sum();
            if (sum <= 0)
            {
                // all zero → uniform among remaining
                var pickU = remaining.Keys.OrderBy(_ => rng.Next()).First();
                picked.Add(pickU);
                remaining.Remove(pickU);
                continue;
            }

            var roll = rng.NextDouble() * sum;
            double acc = 0;
            int chosen = -1;

            foreach (var (n, w) in remaining)
            {
                acc += w;
                if (roll <= acc)
                {
                    chosen = n;
                    break;
                }
            }

            if (chosen == -1)
                chosen = remaining.Keys.Last(); // numeric safety

            picked.Add(chosen);
            remaining.Remove(chosen);
        }

        // small shuffle for variety
        return picked.OrderBy(_ => rng.Next()).Take(count).ToImmutableArray();
    }

    public static double CalculateWeightedProbabilityConfidence(
        IReadOnlyCollection<HistoricalDraw> historicalDraws,
        Dictionary<int, double> weights)
    {
        if (historicalDraws.Count == 0) return 0d;

        // Sum of weights for numbers that appeared historically,
        // normalized by total weight mass across all draws.
        double matched = 0;
        foreach (var d in historicalDraws)
        foreach (var n in d.WinningNumbers)
            if (weights.TryGetValue(n, out var w)) matched += w;

        var totalMass = historicalDraws.Count * weights.Values.Sum(); // == historical draws × 1.0
        return totalMass <= 0 ? 0d : matched / totalMass;              // 0..1
    }

    public static ImmutableArray<int> RandomDistinct(
        int minInclusive,
        int maxInclusive,
        ImmutableArray<int> exclude,
        int count,
        Random rng)
    {
        if (count <= 0) return ImmutableArray<int>.Empty;

        var candidates = Enumerable.Range(minInclusive, maxInclusive - minInclusive + 1)
            .Except(exclude);
        return candidates.OrderBy(_ => rng.Next())
            .Take(count)
            .ToImmutableArray();
    }
}