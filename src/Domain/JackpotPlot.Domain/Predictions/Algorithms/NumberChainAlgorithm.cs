using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.ValueObjects;
using System.Collections.Immutable;
using JackpotPlot.Domain.Predictions.Helpers;
using JackpotPlot.Primitives.Algorithms;

namespace JackpotPlot.Domain.Predictions.Algorithms;

[PredictionAlgorithmDescription(PredictionAlgorithmKeys.NumberChain, "Identifies chains or sequences of numbers that often appear together in historical draws and uses these chains to inform the predicted numbers.")]
public sealed class NumberChainAlgorithm : IPredictionAlgorithm
{
    public PredictionResult Predict(LotteryConfigurationDomain config, IReadOnlyList<HistoricalDraw> history, Random rng)
    {
        if (history.Count == 0)
        {
            return new PredictionResult(
                config.LotteryId,
                ImmutableArray<int>.Empty,
                ImmutableArray<int>.Empty,
                0,
                PredictionAlgorithmKeys.NumberChain);
        }

        // 1) analyze chains from history (pairs + triplets)
        var chains = NumberChainAlgorithmHelpers.AnalyzeNumberChains(history);

        // 2) pick main numbers from most frequent chains
        var main = NumberChainAlgorithmHelpers.GenerateNumbersFromChains(chains, config.MainNumbersCount, rng)
            .ToImmutableArray();

        // 3) bonus numbers (random, distinct from main if you want)
        var bonus = config.BonusNumbersCount > 0
            ? NumberChainAlgorithmHelpers.GenerateRandomNumbers(1, config.BonusNumbersRange, main, config.BonusNumbersCount, rng)
            : ImmutableArray<int>.Empty;

        // 4) confidence: blend chain match rate + historical overlap
        var confidence = NumberChainAlgorithmHelpers.CalculateChainConfidence(history, main.ToList(), chains);

        return new PredictionResult(
            config.LotteryId,
            main,
            bonus,
            confidence,
            PredictionAlgorithmKeys.NumberChain);
    }
}