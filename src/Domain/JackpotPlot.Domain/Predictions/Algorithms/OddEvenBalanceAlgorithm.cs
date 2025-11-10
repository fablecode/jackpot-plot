using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.ValueObjects;
using System.Collections.Immutable;
using JackpotPlot.Domain.Predictions.Helpers;
using JackpotPlot.Primitives.Algorithms;

namespace JackpotPlot.Domain.Predictions.Algorithms;

[PredictionAlgorithmDescription(PredictionAlgorithmKeys.OddEvenBalance, "Balances predictions by ensuring a similar proportion of odd and even numbers as observed in historical draws.")]
public sealed class OddEvenBalanceAlgorithm : IPredictionAlgorithm
{
    public PredictionResult Predict(
        LotteryConfigurationDomain config,
        IReadOnlyList<HistoricalDraw> history,
        Random rng)
    {
        // No history → even split fallback
        if (history.Count == 0)
        {
            var oddCount = config.MainNumbersCount / 2;
            var evenCount = config.MainNumbersCount - oddCount;

            var odds = OddEvenBalanceAlgorithmHelpers.GenerateNumbers(1, config.MainNumbersRange, n => (n & 1) == 1, oddCount, rng);
            var evens = OddEvenBalanceAlgorithmHelpers.GenerateNumbers(1, config.MainNumbersRange, n => (n & 1) == 0, evenCount, rng);

            var mainFallback = odds.Concat(evens).OrderBy(_ => rng.Next()).ToImmutableArray();
            var bonusFallback = config.BonusNumbersCount > 0
                ? OddEvenBalanceAlgorithmHelpers.GenerateNumbers(1, config.BonusNumbersRange, _ => true, config.BonusNumbersCount, rng)
                : ImmutableArray<int>.Empty;

            return new PredictionResult(
                config.LotteryId, mainFallback, bonusFallback, 0d, PredictionAlgorithmKeys.OddEvenBalance);
        }

        // 1) Compute historical odd/even ratios
        var (oddRatio, evenRatio) = OddEvenBalanceAlgorithmHelpers.CalculateOddEvenRatio(history);

        // 2) Compute counts for this draw
        var oddCountTarget = (int)Math.Round(config.MainNumbersCount * oddRatio, MidpointRounding.AwayFromZero);
        oddCountTarget = Math.Clamp(oddCountTarget, 0, config.MainNumbersCount);
        var evenCountTarget = config.MainNumbersCount - oddCountTarget;

        // 3) Sample odd/even numbers within range (distinct)
        var oddNumbers = OddEvenBalanceAlgorithmHelpers.GenerateNumbers(1, config.MainNumbersRange, n => (n & 1) == 1, oddCountTarget, rng);
        var evenNumbers = OddEvenBalanceAlgorithmHelpers.GenerateNumbers(1, config.MainNumbersRange, n => (n & 1) == 0, evenCountTarget, rng);

        var main = oddNumbers.Concat(evenNumbers).OrderBy(_ => rng.Next()).ToImmutableArray();

        // 4) Bonus numbers (random, distinct not required unless you want to exclude)
        var bonus = config.BonusNumbersCount > 0
            ? OddEvenBalanceAlgorithmHelpers.GenerateNumbers(1, config.BonusNumbersRange, _ => true, config.BonusNumbersCount, rng)
            : ImmutableArray<int>.Empty;

        // 5) Confidence = frequency of historical draws matching same odd/even split
        var confidence = OddEvenBalanceAlgorithmHelpers.CalculateOddEvenBalanceConfidence(history, main.ToList());

        return new PredictionResult(
            config.LotteryId,
            main,
            bonus,
            confidence,
            PredictionAlgorithmKeys.OddEvenBalance);
    }
}