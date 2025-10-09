using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.ValueObjects;
using System.Collections.Immutable;
using JackpotPlot.Primitives.Algorithms;

namespace JackpotPlot.Domain.Predictions.Algorithms;

[PredictionAlgorithmDescription(PredictionAlgorithmKeys.WeightedProbability, "Assigns weights to each number based on its historical frequency and selects numbers through weighted random sampling, favoring those with higher or lower weights as needed.")]
public sealed class WeightedProbabilityAlgorithm : IPredictionAlgorithm
{
    public PredictionResult Predict(
        LotteryConfigurationDomain config,
        IReadOnlyList<HistoricalDraw> history,
        Random rng)
    {
        // No history → empty (or swap to uniform random if you prefer)
        if (history.Count == 0)
        {
            return new PredictionResult(
                config.LotteryId,
                ImmutableArray<int>.Empty,
                ImmutableArray<int>.Empty,
                0d,
                PredictionAlgorithmKeys.WeightedProbability);
        }

        // 1) weights over [1..MainNumbersRange]
        var weights = CalculateWeights(history, config.MainNumbersRange);

        // 2) main numbers: weighted sample without replacement
        var main = WeightedSampleDistinct(weights, config.MainNumbersCount, rng).ToImmutableArray();

        // 3) bonus numbers: uniform random & distinct from main
        var bonus = config.BonusNumbersCount > 0
            ? RandomDistinct(1, config.BonusNumbersRange, main, config.BonusNumbersCount, rng)
            : ImmutableArray<int>.Empty;

        // 4) confidence: historical overlap weighted by probabilities
        var confidence = CalculateWeightedProbabilityConfidence(history, weights);

        return new PredictionResult(
            config.LotteryId,
            main,
            bonus,
            confidence,
            PredictionAlgorithmKeys.WeightedProbability);
    }

    // ---------- helpers (PURE) ----------

    private static Dictionary<int, double> CalculateWeights(
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

    private static ImmutableArray<int> WeightedSampleDistinct(
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

    private static double CalculateWeightedProbabilityConfidence(
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

    private static ImmutableArray<int> RandomDistinct(
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