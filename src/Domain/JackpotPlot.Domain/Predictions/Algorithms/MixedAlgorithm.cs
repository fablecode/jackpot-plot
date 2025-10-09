using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.ValueObjects;
using System.Collections.Immutable;
using JackpotPlot.Primitives.Algorithms;

namespace JackpotPlot.Domain.Predictions.Algorithms;

[PredictionAlgorithmDescription(PredictionAlgorithmKeys.Mixed, "Combines multiple prediction strategies, weighting their outputs to generate a more robust overall prediction.")]
public sealed class MixedAlgorithm : IPredictionAlgorithm
{
    private readonly IReadOnlyList<(IPredictionAlgorithm Algo, double Weight)> _components;

    /// <param name="components">
    /// Algorithms to compose with their weights. Weights may be any non-negative doubles.
    /// </param>
    public MixedAlgorithm(IReadOnlyList<(IPredictionAlgorithm Algo, double Weight)> components)
    {
        _components = components ?? throw new ArgumentNullException(nameof(components));
    }

    public PredictionResult Predict(
        LotteryConfigurationDomain config,
        IReadOnlyList<HistoricalDraw> history,
        Random rng)
    {
        if (_components.Count == 0)
        {
            // no components => empty result
            return new PredictionResult(
                config.LotteryId,
                ImmutableArray<int>.Empty,
                ImmutableArray<int>.Empty,
                0,
                PredictionAlgorithmKeys.Mixed);
        }

        // 1) Run each component algorithm
        var results = new List<(PredictionResult Result, double Weight)>(_components.Count);
        foreach (var (algo, weight) in _components)
        {
            var r = algo.Predict(config, history, rng);
            results.Add((r, Math.Max(0, weight)));
        }

        // 2) Combine main numbers by weighted vote
        var main = CombineMainNumbers(results, config.MainNumbersCount, rng)
            .ToImmutableArray();

        // 3) Combine bonus numbers by unweighted frequency (to mirror prior behavior)
        var bonus = config.BonusNumbersCount > 0
            ? CombineBonusNumbers(results, config.BonusNumbersCount).ToImmutableArray()
            : ImmutableArray<int>.Empty;

        // 4) Weighted average confidence
        var confidence = WeightedAverageConfidence(results);

        return new PredictionResult(
            config.LotteryId,
            main,
            bonus,
            confidence,
            PredictionAlgorithmKeys.Mixed);
    }

    // ---------- helpers (pure) ----------

    private static IEnumerable<int> CombineMainNumbers(
        List<(PredictionResult Result, double Weight)> results,
        int take,
        Random rng)
    {
        if (take <= 0) yield break;

        var weights = new Dictionary<int, double>();

        foreach (var (res, w) in results)
        {
            var weight = w > 0 ? w : 0;
            foreach (var n in res.PredictedNumbers)
            {
                if (!weights.TryAdd(n, weight))
                    weights[n] += weight;
            }
        }

        // order by weight desc; break ties with randomness for variety
        foreach (var n in weights
                     .OrderByDescending(kv => kv.Value)
                     .ThenBy(_ => rng.Next())
                     .Select(kv => kv.Key)
                     .Take(take))
        {
            yield return n;
        }
    }

    private static IEnumerable<int> CombineBonusNumbers(
        List<(PredictionResult Result, double Weight)> results,
        int take)
    {
        if (take <= 0) yield break;

        var counts = results
            .SelectMany(t => t.Result.BonusNumbers)
            .GroupBy(n => n)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .Take(take);

        foreach (var n in counts) yield return n;
    }

    private static double WeightedAverageConfidence(List<(PredictionResult Result, double Weight)> results)
    {
        double total = 0, wsum = 0;
        foreach (var (r, w) in results)
        {
            total += r.ConfidenceScore * w;
            wsum += w;
        }
        return wsum == 0 ? 0 : total / wsum;
    }
}