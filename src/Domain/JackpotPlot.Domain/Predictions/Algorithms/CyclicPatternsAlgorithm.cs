using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.ValueObjects;
using System.Collections.Immutable;
using JackpotPlot.Primitives.Algorithms;

namespace JackpotPlot.Domain.Predictions.Algorithms;

[PredictionAlgorithmDescription(PredictionAlgorithmKeys.CyclicPatterns, "Identifies recurring cycles or intervals in historical draws and predicts numbers that are due to appear based on these cyclic patterns.")]
public sealed class CyclicPatternsAlgorithm : IPredictionAlgorithm
{
    public PredictionResult Predict(
        LotteryConfigurationDomain config,
        IReadOnlyList<HistoricalDraw> history,
        Random rng)
    {
        if (history.Count == 0)
            return new PredictionResult(
                config.LotteryId, ImmutableArray<int>.Empty, ImmutableArray<int>.Empty, 0,
                PredictionAlgorithmKeys.CyclicPatterns);

        // 1) analyze cycles (distance between appearances for each number in range)
        var cycles = AnalyzeCyclicPatterns(history, config.MainNumbersRange);

        // 2) choose main numbers by shortest average cycle first
        var main = GenerateNumbersFromCycles(cycles, config.MainNumbersCount, rng).ToImmutableArray();

        // 3) bonus numbers (random, distinct from main)
        var bonus = config.BonusNumbersCount > 0
            ? GenerateRandomNumbers(1, config.BonusNumbersRange, main, config.BonusNumbersCount, rng)
            : ImmutableArray<int>.Empty;

        // 4) naive confidence: fraction of predicted numbers that are "due" per their average cycle
        var confidence = CalculateCyclicConfidence(history, main, cycles);

        return new PredictionResult(
            config.LotteryId,
            main,
            bonus,
            confidence,
            PredictionAlgorithmKeys.CyclicPatterns);
    }

    // ---------- helpers (pure) ----------

    private static Dictionary<int, List<int>> AnalyzeCyclicPatterns(IReadOnlyList<HistoricalDraw> draws, int numberRange)
    {
        var cycles = new Dictionary<int, List<int>>(capacity: numberRange);

        for (int number = 1; number <= numberRange; number++)
        {
            var gaps = new List<int>();
            var lastIdx = -1;

            for (int i = 0; i < draws.Count; i++)
            {
                if (draws[i].WinningNumbers.Contains(number))
                {
                    if (lastIdx != -1) gaps.Add(i - lastIdx);
                    lastIdx = i;
                }
            }

            cycles[number] = gaps;
        }

        return cycles;
    }

    private static List<int> GenerateNumbersFromCycles(
        Dictionary<int, List<int>> cycles,
        int count,
        Random rng)
    {
        // order numbers by the shortest average cycle (more "frequent" cycle → likely due)
        var ordered = cycles
            .Where(kv => kv.Value.Count > 0)
            .OrderBy(kv => kv.Value.Average())
            .Select(kv => kv.Key)
            .Take(count)
            .ToList();

        // shuffle for a touch of randomness / tie-breaking
        return ordered.OrderBy(_ => rng.Next()).ToList();
    }

    private static double CalculateCyclicConfidence(
        IReadOnlyList<HistoricalDraw> draws,
        ImmutableArray<int> predicted,
        Dictionary<int, List<int>> cycles)
    {
        if (predicted.Length == 0) return 0;

        int dueCount = 0;

        foreach (var number in predicted)
        {
            if (!cycles.TryGetValue(number, out var gaps) || gaps.Count == 0)
                continue;

            // average cycle distance for this number
            var avg = (int)Math.Round(gaps.Average());

            // find the last time this number appeared (search from the end)
            var lastIdx = -1;
            for (int i = draws.Count - 1; i >= 0; i--)
            {
                if (draws[i].WinningNumbers.Contains(number))
                {
                    lastIdx = i;
                    break;
                }
            }

            if (lastIdx != -1)
            {
                var distanceSinceLast = draws.Count - 1 - lastIdx;
                if (distanceSinceLast >= avg) dueCount++;
            }
        }

        return (double)dueCount / predicted.Length;
    }

    private static ImmutableArray<int> GenerateRandomNumbers(
        int minInclusive,
        int maxInclusive,
        ImmutableArray<int> exclude,
        int count,
        Random rng)
    {
        if (count <= 0) return ImmutableArray<int>.Empty;

        var candidates = Enumerable
            .Range(minInclusive, maxInclusive - minInclusive + 1)
            .Except(exclude);

        return candidates
            .OrderBy(_ => rng.Next())
            .Take(count)
            .ToImmutableArray();
    }
}