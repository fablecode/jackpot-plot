using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.ValueObjects;
using System.Collections.Immutable;
using JackpotPlot.Primitives.Algorithms;

namespace JackpotPlot.Domain.Predictions.Algorithms;

[PredictionAlgorithmDescription(PredictionAlgorithmKeys.Random, "Generate numbers randomly.")]
public sealed class RandomAlgorithm : IPredictionAlgorithm
{
    public PredictionResult Predict(
        LotteryConfigurationDomain config,
        IReadOnlyList<HistoricalDraw> history, // unused, but kept for interface consistency
        Random rng)
    {
        // main numbers (distinct, uniform within range)
        var main = Enumerable.Range(1, config.MainNumbersRange)
            .OrderBy(_ => rng.Next())
            .Take(config.MainNumbersCount)
            .ToImmutableArray();

        // bonus numbers (optional, distinct, uniform within bonus range)
        var bonus = config.BonusNumbersCount > 0
            ? Enumerable.Range(1, config.BonusNumbersRange)
                .OrderBy(_ => rng.Next())
                .Take(config.BonusNumbersCount)
                .ToImmutableArray()
            : ImmutableArray<int>.Empty;

        // same confidence idea as before: simple probability proxy
        var confidence = CalculateRandomConfidence(config.MainNumbersRange, config.MainNumbersCount);

        return new PredictionResult(
            config.LotteryId,
            main,
            bonus,
            confidence,
            PredictionAlgorithmKeys.Random);
    }

    private static double CalculateRandomConfidence(int mainNumbersRange, int mainNumbersCount)
    {
        // mirrors original: probability proxy of a single correct hit
        return 1.0 / (mainNumbersRange - mainNumbersCount + 1);
    }
}