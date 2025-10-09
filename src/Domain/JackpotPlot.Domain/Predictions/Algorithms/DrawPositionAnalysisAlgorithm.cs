using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.ValueObjects;
using System.Collections.Immutable;
using JackpotPlot.Primitives.Algorithms;

namespace JackpotPlot.Domain.Predictions.Algorithms;

[PredictionAlgorithmDescription(PredictionAlgorithmKeys.DrawPositionAnalysis, "Evaluates the positions in which numbers appear (first, second, etc.) in historical draws to predict numbers that are likely to occur in those same positions.")]
public sealed class DrawPositionAnalysisAlgorithm : IPredictionAlgorithm
{
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
                0,
                PredictionAlgorithmKeys.DrawPositionAnalysis);
        }

        // 1) compute per-position frequency tables for main numbers
        var positionFrequencies = AnalyzeDrawPositions(
            history, config.MainNumbersCount, config.MainNumbersRange);

        // 2) choose one number per position (break ties with rng), then shuffle a bit
        var main = GenerateNumbersFromPositions(positionFrequencies, rng)
            .Take(config.MainNumbersCount)
            .ToImmutableArray();

        // 3) bonus numbers (random & distinct from main)
        var bonus = config.BonusNumbersCount > 0
            ? GenerateRandomNumbers(1, config.BonusNumbersRange, main, config.BonusNumbersCount, rng)
            : ImmutableArray<int>.Empty;

        // 4) simple confidence: fraction of exact position matches historically
        var confidence = CalculatePositionConfidence(history, main);

        return new PredictionResult(
            config.LotteryId,
            main,
            bonus,
            confidence,
            PredictionAlgorithmKeys.DrawPositionAnalysis);
    }

    // ---------- helpers (PURE) ----------

    private static Dictionary<int, Dictionary<int, int>> AnalyzeDrawPositions(
        IReadOnlyCollection<HistoricalDraw> draws,
        int mainNumbersCount,
        int numberRange)
    {
        var positionFrequencies = new Dictionary<int, Dictionary<int, int>>(mainNumbersCount);

        // init frequency tables
        for (int pos = 0; pos < mainNumbersCount; pos++)
        {
            var freq = new Dictionary<int, int>(numberRange);
            for (int n = 1; n <= numberRange; n++) freq[n] = 0;
            positionFrequencies[pos] = freq;
        }

        // accumulate
        foreach (var draw in draws)
        {
            for (int pos = 0; pos < mainNumbersCount && pos < draw.WinningNumbers.Count; pos++)
            {
                var number = draw.WinningNumbers[pos];
                positionFrequencies[pos][number]++;
            }
        }

        return positionFrequencies;
    }

    private static IEnumerable<int> GenerateNumbersFromPositions(
        Dictionary<int, Dictionary<int, int>> positionFrequencies,
        Random rng)
    {
        var selected = new List<int>();

        foreach (var pos in positionFrequencies.Keys.OrderBy(p => p))
        {
            // pick most frequent for this position; break ties with rng
            var pick = positionFrequencies[pos]
                .OrderByDescending(kv => kv.Value)
                .ThenBy(_ => rng.Next())
                .Select(kv => kv.Key)
                .First();

            if (!selected.Contains(pick))
                selected.Add(pick);
        }

        // small shuffle so output isn’t always monotonically increasing by position
        return selected.OrderBy(_ => rng.Next());
    }

    private static double CalculatePositionConfidence(
        IReadOnlyCollection<HistoricalDraw> draws,
        ImmutableArray<int> predicted)
    {
        if (draws.Count == 0 || predicted.Length == 0) return 0;

        int matches = 0;
        int totalPositions = 0;

        foreach (var d in draws)
        {
            var limit = Math.Min(predicted.Length, d.WinningNumbers.Count);
            totalPositions += limit;

            for (int pos = 0; pos < limit; pos++)
                if (d.WinningNumbers[pos] == predicted[pos]) matches++;
        }

        return totalPositions == 0 ? 0 : (double)matches / totalPositions;
    }

    private static ImmutableArray<int> GenerateRandomNumbers(
        int minInclusive,
        int maxInclusive,
        ImmutableArray<int> exclude,
        int count,
        Random rng)
    {
        if (count <= 0) return ImmutableArray<int>.Empty;

        var candidates = Enumerable.Range(minInclusive, maxInclusive - minInclusive + 1)
            .Except(exclude);

        return candidates
            .OrderBy(_ => rng.Next())
            .Take(count)
            .ToImmutableArray();
    }
}