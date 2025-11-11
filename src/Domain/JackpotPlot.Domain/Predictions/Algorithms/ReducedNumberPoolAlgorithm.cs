using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.ValueObjects;
using System.Collections.Immutable;
using JackpotPlot.Domain.Predictions.Helpers;
using JackpotPlot.Primitives.Algorithms;

namespace JackpotPlot.Domain.Predictions.Algorithms;

[PredictionAlgorithmDescription(PredictionAlgorithmKeys.ReducedNumberPool, "Narrows the candidate pool by excluding numbers that appear infrequently, thereby focusing predictions on historically more likely numbers.")]
public sealed class ReducedNumberPoolAlgorithm : IPredictionAlgorithm
{
    private readonly double _appearanceThresholdRatio;

    /// <param name="appearanceThresholdRatio">
    /// Ratio (0..1] of draws a number must appear in to remain in the pool.
    /// Default is 0.10 (10%), matching the original strategy.
    /// </param>
    public ReducedNumberPoolAlgorithm(double appearanceThresholdRatio = 0.10)
    {
        _appearanceThresholdRatio = Math.Clamp(appearanceThresholdRatio, 0, 1);
    }

    public PredictionResult Predict(
        LotteryConfigurationDomain config,
        IReadOnlyList<HistoricalDraw> history,
        Random rng)
    {
        // 1) Build reduced pool from history
        var reducedPool = ReducedNumberPoolAlgorithmHelpers.AnalyzeReducedNumberPool(history, config.MainNumbersRange, _appearanceThresholdRatio);

        // 2) Generate main numbers (fill from full range if pool is too small)
        var main = ReducedNumberPoolAlgorithmHelpers.GenerateFromReducedPool(
                reducedPool, config.MainNumbersRange, config.MainNumbersCount, rng)
            .ToImmutableArray();

        // 3) Bonus numbers (random, distinct from main)
        var bonus = config.BonusNumbersCount > 0
            ? ReducedNumberPoolAlgorithmHelpers.RandomDistinct(1, config.BonusNumbersRange, main, config.BonusNumbersCount, rng)
            : ImmutableArray<int>.Empty;

        // 4) Confidence: historical overlap rate with predicted numbers
        var confidence = ReducedNumberPoolAlgorithmHelpers.CalculateReducedPoolConfidence(history, main.ToList());

        return new PredictionResult(
            config.LotteryId,
            main,
            bonus,
            confidence,
            PredictionAlgorithmKeys.ReducedNumberPool);
    }
}