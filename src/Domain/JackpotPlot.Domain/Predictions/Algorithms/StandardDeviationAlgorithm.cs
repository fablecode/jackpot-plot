using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.ValueObjects;
using System.Collections.Immutable;
using JackpotPlot.Primitives.Algorithms;

namespace JackpotPlot.Domain.Predictions.Algorithms;

[PredictionAlgorithmDescription(PredictionAlgorithmKeys.StandardDeviation, "Uses the spread (standard deviation) of historical draws to generate predictions that mimic the overall variability in the numbers.")]
public sealed class StandardDeviationAlgorithm : IPredictionAlgorithm
{
    public PredictionResult Predict(
        LotteryConfigurationDomain config,
        IReadOnlyList<HistoricalDraw> history,
        Random rng)
    {
        // no history → empty
        if (history.Count == 0)
        {
            return new PredictionResult(
                config.LotteryId,
                ImmutableArray<int>.Empty,
                ImmutableArray<int>.Empty,
                0d,
                PredictionAlgorithmKeys.StandardDeviation);
        }

        // 1) calculate historical std deviation
        var targetStdDev = CalculateHistoricalStandardDeviation(history);

        // 2) generate numbers approximating that std dev
        var main = GenerateNumbersWithTargetStdDev(
                config.MainNumbersRange,
                config.MainNumbersCount,
                targetStdDev,
                rng)
            .ToImmutableArray();

        // 3) bonus numbers (random distinct)
        var bonus = config.BonusNumbersCount > 0
            ? RandomDistinct(1, config.BonusNumbersRange, main, config.BonusNumbersCount, rng)
            : ImmutableArray<int>.Empty;

        // 4) confidence = closeness of predicted vs historical std dev
        var confidence = CalculateStandardDeviationConfidence(history, main.ToList());

        return new PredictionResult(
            config.LotteryId,
            main,
            bonus,
            confidence,
            PredictionAlgorithmKeys.StandardDeviation);
    }

    // ---------- helpers (PURE) ----------

    private static double CalculateHistoricalStandardDeviation(IEnumerable<HistoricalDraw> draws)
    {
        var all = draws.SelectMany(d => d.WinningNumbers).Select(n => (double)n).ToList();
        if (all.Count == 0) return 0;

        var mean = all.Average();
        var variance = all.Sum(x => Math.Pow(x - mean, 2)) / all.Count;
        return Math.Sqrt(variance);
    }

    private static List<int> GenerateNumbersWithTargetStdDev(
        int maxRange,
        int count,
        double targetStdDev,
        Random rng)
    {
        if (count <= 0) return new();

        var numbers = new List<int>();

        while (numbers.Count < count)
        {
            int candidate = rng.Next(1, maxRange + 1);

            var candidateSet = numbers.Concat(new[] { candidate }).ToList();
            double currentStdDev = CalculateStandardDeviation(candidateSet);

            // accept if close enough to target or list empty
            if (numbers.Count == 0 || Math.Abs(currentStdDev - targetStdDev) < 0.5)
                numbers.Add(candidate);

            numbers = numbers.Distinct().ToList();
        }

        // shuffle for variety
        return numbers.OrderBy(_ => rng.Next()).Take(count).ToList();
    }

    private static double CalculateStandardDeviation(List<int> numbers)
    {
        if (numbers.Count == 0) return 0;
        var mean = numbers.Average();
        var variance = numbers.Sum(n => Math.Pow(n - mean, 2)) / numbers.Count;
        return Math.Sqrt(variance);
    }

    private static double CalculateStandardDeviationConfidence(
        IEnumerable<HistoricalDraw> draws,
        List<int> predicted)
    {
        if (!draws.Any() || predicted.Count == 0) return 0d;

        var histStdDev = CalculateHistoricalStandardDeviation(draws);
        var predStdDev = CalculateStandardDeviation(predicted);

        // confidence: 1 / (1 + difference)
        return 1.0 / (1.0 + Math.Abs(histStdDev - predStdDev));
    }

    private static ImmutableArray<int> RandomDistinct(
        int minInclusive,
        int maxInclusive,
        ImmutableArray<int> exclude,
        int count,
        Random rng)
    {
        if (count <= 0) return ImmutableArray<int>.Empty;

        var pool = Enumerable.Range(minInclusive, maxInclusive - minInclusive + 1)
            .Except(exclude);
        return pool.OrderBy(_ => rng.Next())
            .Take(count)
            .ToImmutableArray();
    }
}