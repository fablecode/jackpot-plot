using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.ValueObjects;
using System.Collections.Immutable;
using JackpotPlot.Domain.Predictions.Helpers;
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
        var main = MixedAlgorithmHelpers.CombineMainNumbers(results, config.MainNumbersCount, rng)
            .ToImmutableArray();

        // 3) Combine bonus numbers by unweighted frequency (to mirror prior behavior)
        var bonus = config.BonusNumbersCount > 0
            ? MixedAlgorithmHelpers.CombineBonusNumbers(results, config.BonusNumbersCount).ToImmutableArray()
            : ImmutableArray<int>.Empty;

        // 4) Weighted average confidence
        var confidence = MixedAlgorithmHelpers.WeightedAverageConfidence(results);

        return new PredictionResult(
            config.LotteryId,
            main,
            bonus,
            confidence,
            PredictionAlgorithmKeys.Mixed);
    }
}