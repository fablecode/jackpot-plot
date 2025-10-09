using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.ValueObjects;
using System.Collections.Immutable;
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

            var odds = GenerateNumbers(1, config.MainNumbersRange, n => (n & 1) == 1, oddCount, rng);
            var evens = GenerateNumbers(1, config.MainNumbersRange, n => (n & 1) == 0, evenCount, rng);

            var mainFallback = odds.Concat(evens).OrderBy(_ => rng.Next()).ToImmutableArray();
            var bonusFallback = config.BonusNumbersCount > 0
                ? GenerateNumbers(1, config.BonusNumbersRange, _ => true, config.BonusNumbersCount, rng)
                : ImmutableArray<int>.Empty;

            return new PredictionResult(
                config.LotteryId, mainFallback, bonusFallback, 0d, PredictionAlgorithmKeys.OddEvenBalance);
        }

        // 1) Compute historical odd/even ratios
        var (oddRatio, evenRatio) = CalculateOddEvenRatio(history);

        // 2) Compute counts for this draw
        var oddCountTarget = (int)Math.Round(config.MainNumbersCount * oddRatio, MidpointRounding.AwayFromZero);
        oddCountTarget = Math.Clamp(oddCountTarget, 0, config.MainNumbersCount);
        var evenCountTarget = config.MainNumbersCount - oddCountTarget;

        // 3) Sample odd/even numbers within range (distinct)
        var oddNumbers = GenerateNumbers(1, config.MainNumbersRange, n => (n & 1) == 1, oddCountTarget, rng);
        var evenNumbers = GenerateNumbers(1, config.MainNumbersRange, n => (n & 1) == 0, evenCountTarget, rng);

        var main = oddNumbers.Concat(evenNumbers).OrderBy(_ => rng.Next()).ToImmutableArray();

        // 4) Bonus numbers (random, distinct not required unless you want to exclude)
        var bonus = config.BonusNumbersCount > 0
            ? GenerateNumbers(1, config.BonusNumbersRange, _ => true, config.BonusNumbersCount, rng)
            : ImmutableArray<int>.Empty;

        // 5) Confidence = frequency of historical draws matching same odd/even split
        var confidence = CalculateOddEvenBalanceConfidence(history, main.ToList());

        return new PredictionResult(
            config.LotteryId,
            main,
            bonus,
            confidence,
            PredictionAlgorithmKeys.OddEvenBalance);
    }

    // ---------- helpers (PURE) ----------

    private static (double oddRatio, double evenRatio) CalculateOddEvenRatio(IEnumerable<HistoricalDraw> historicalDraws)
    {
        int odd = 0, even = 0;
        foreach (var d in historicalDraws)
        {
            foreach (var n in d.WinningNumbers)
            {
                if ((n & 1) == 1) odd++;
                else even++;
            }
        }

        var total = odd + even;
        return total == 0 ? (0.5, 0.5) : (odd / (double)total, even / (double)total);
    }

    private static ImmutableArray<int> GenerateNumbers(
        int minInclusive,
        int maxInclusive,
        Func<int, bool> predicate,
        int count,
        Random rng)
    {
        if (count <= 0) return ImmutableArray<int>.Empty;

        var pool = Enumerable.Range(minInclusive, maxInclusive - minInclusive + 1)
            .Where(predicate);

        return pool.OrderBy(_ => rng.Next())
            .Take(count)
            .ToImmutableArray();
    }

    private static double CalculateOddEvenBalanceConfidence(
        IReadOnlyCollection<HistoricalDraw> historicalDraws,
        List<int> predictedNumbers)
    {
        if (historicalDraws.Count == 0 || predictedNumbers.Count == 0) return 0d;

        var predictedOdd = predictedNumbers.Count(n => (n & 1) == 1);
        var predictedEven = predictedNumbers.Count - predictedOdd;

        var matches = 0;
        foreach (var draw in historicalDraws)
        {
            var odd = draw.WinningNumbers.Count(n => (n & 1) == 1);
            var even = draw.WinningNumbers.Count - odd;
            if (odd == predictedOdd && even == predictedEven)
                matches++;
        }

        return matches / (double)historicalDraws.Count;
    }
}