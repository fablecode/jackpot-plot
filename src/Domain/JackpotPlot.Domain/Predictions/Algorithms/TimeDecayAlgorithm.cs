using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.ValueObjects;
using System.Collections.Immutable;
using JackpotPlot.Primitives.Algorithms;

namespace JackpotPlot.Domain.Predictions.Algorithms;

[PredictionAlgorithmDescription(PredictionAlgorithmKeys.TimeDecay, "Weighs recent draws more heavily than older ones, using a decay function so that the most current trends have greater influence on the prediction.")]
public sealed class TimeDecayAlgorithm : IPredictionAlgorithm
{
    private readonly double _decayFactor;

    public TimeDecayAlgorithm(double decayFactor = 0.9)
    {
        // clamp to (0,1]; lower means faster decay
        _decayFactor = Math.Clamp(decayFactor, 0.0001, 1.0);
    }

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
                PredictionAlgorithmKeys.TimeDecay);
        }

        // assign decayed weights: newest draw gets highest weight
        var weightsById = AssignTimeDecayWeights(history, _decayFactor);

        // build weighted frequency over [1..MainNumbersRange]
        var weighted = CalculateWeightedFrequencies(history, weightsById, config.MainNumbersRange);

        // sample main numbers proportionally to weight (without replacement)
        var main = WeightedSampleDistinct(weighted, config.MainNumbersCount, rng).ToImmutableArray();

        // bonus numbers random & distinct from main
        var bonus = config.BonusNumbersCount > 0
            ? RandomDistinct(1, config.BonusNumbersRange, main, config.BonusNumbersCount, rng)
            : ImmutableArray<int>.Empty;

        // confidence: how much we match the last 10 draws (like original)
        var confidence = TimeDecayConfidence(history, main.ToList());

        return new PredictionResult(
            config.LotteryId, main, bonus, confidence, PredictionAlgorithmKeys.TimeDecay);
    }

    // ---------- helpers (PURE) ----------

    private static Dictionary<int, double> AssignTimeDecayWeights(
        IEnumerable<HistoricalDraw> draws, double decay)
    {
        var map = new Dictionary<int, double>();
        int idx = 0;
        foreach (var d in draws.OrderByDescending(d => d.DrawDate))
            map[d.DrawId] = Math.Pow(decay, idx++);
        return map;
    }

    private static Dictionary<int, double> CalculateWeightedFrequencies(
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

    private static IEnumerable<int> WeightedSampleDistinct(
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

    private static double TimeDecayConfidence(IReadOnlyList<HistoricalDraw> history, List<int> predicted)
    {
        if (predicted.Count == 0) return 0d;

        var recent = history.OrderByDescending(d => d.DrawDate).Take(10).ToList();
        if (recent.Count == 0) return 0d;

        var matches = recent.Sum(d => d.WinningNumbers.Intersect(predicted).Count());
        return (double)matches / (recent.Count * predicted.Count);
    }

    private static ImmutableArray<int> RandomDistinct(
        int minInclusive, int maxInclusive,
        ImmutableArray<int> exclude, int count, Random rng)
    {
        if (count <= 0) return ImmutableArray<int>.Empty;
        var candidates = Enumerable.Range(minInclusive, maxInclusive - minInclusive + 1)
            .Except(exclude);
        return candidates.OrderBy(_ => rng.Next()).Take(count).ToImmutableArray();
    }
}