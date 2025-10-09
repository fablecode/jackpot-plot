using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.ValueObjects;
using System.Collections.Immutable;
using JackpotPlot.Primitives.Algorithms;

namespace JackpotPlot.Domain.Predictions.Algorithms;

[PredictionAlgorithmDescription(PredictionAlgorithmKeys.WeightDistribution, "Similar to weighted probability, this strategy calculates and applies weights to numbers based on historical data to select numbers proportionally from their weight distribution.")]
public sealed class WeightDistributionAlgorithm : IPredictionAlgorithm
{
    public PredictionResult Predict(
        LotteryConfigurationDomain config,
        IReadOnlyList<HistoricalDraw> history,
        Random rng)
    {
        // no history → empty (or fallback to random if you prefer)
        if (history.Count == 0)
        {
            return new PredictionResult(
                config.LotteryId,
                ImmutableArray<int>.Empty,
                ImmutableArray<int>.Empty,
                0d,
                PredictionAlgorithmKeys.WeightDistribution);
        }

        // 1) weights over the main-number range
        var weights = CalculateWeights(history, config.MainNumbersRange);

        // 2) sample main numbers by weight, without replacement
        var main = GenerateWeightedRandomNumbers(weights, config.MainNumbersCount, rng)
            .ToImmutableArray();

        // 3) bonus numbers (random & distinct from main)
        var bonus = config.BonusNumbersCount > 0
            ? RandomDistinct(1, config.BonusNumbersRange, main, config.BonusNumbersCount, rng)
            : ImmutableArray<int>.Empty;

        // 4) confidence: same overlap ratio as the original strategy
        var confidence = CalculateWeightDistributionConfidence(history, main.ToList());

        return new PredictionResult(
            config.LotteryId, main, bonus, confidence, PredictionAlgorithmKeys.WeightDistribution);
    }

    // ---------- helpers (PURE) ----------

    private static Dictionary<int, double> CalculateWeights(
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

    private static ImmutableArray<int> GenerateWeightedRandomNumbers(
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

    private static double CalculateWeightDistributionConfidence(
        IReadOnlyCollection<HistoricalDraw> historicalDraws, List<int> predicted)
    {
        if (historicalDraws.Count == 0 || predicted.Count == 0) return 0d;

        var matches = 0;
        foreach (var d in historicalDraws)
            matches += d.WinningNumbers.Intersect(predicted).Count();

        return (double)matches / (historicalDraws.Count * predicted.Count);
    }

    private static ImmutableArray<int> RandomDistinct(
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