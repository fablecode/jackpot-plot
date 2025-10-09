using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.ValueObjects;
using System.Collections.Immutable;
using JackpotPlot.Primitives.Algorithms;

namespace JackpotPlot.Domain.Predictions.Algorithms;

[PredictionAlgorithmDescription(PredictionAlgorithmKeys.RepeatingNumbers, "Focuses on numbers that have appeared frequently in recent draws, under the assumption that such numbers might continue to repeat.")]
public sealed class RepeatingNumbersAlgorithm : IPredictionAlgorithm
{
    private readonly int _recentDrawsToConsider;

    public RepeatingNumbersAlgorithm(int recentDrawsToConsider = 10)
    {
        _recentDrawsToConsider = Math.Max(1, recentDrawsToConsider);
    }

    public PredictionResult Predict(
        LotteryConfigurationDomain config,
        IReadOnlyList<HistoricalDraw> history,
        Random rng)
    {
        // If no history, return empty (or you could random-fill if desired)
        if (history.Count == 0)
        {
            return new PredictionResult(
                config.LotteryId,
                ImmutableArray<int>.Empty,
                ImmutableArray<int>.Empty,
                0d,
                PredictionAlgorithmKeys.RepeatingNumbers);
        }

        // 1) Collect numbers from the N most recent draws
        // NOTE: assumes 'history' is already ordered newest-first like the original Take(N) usage.
        var recent = GetRecentNumbers(history, _recentDrawsToConsider);

        // 2) Identify repeating numbers (appear > 1 time), ordered by frequency desc
        var repeating = IdentifyRepeatingNumbers(recent);

        // 3) Take top K by frequency for main numbers (like original)
        var main = GeneratePredictionsFromRepeatingNumbers(repeating, config.MainNumbersCount)
            .ToImmutableArray();

        // If fewer than needed (e.g., not enough repeats), top up randomly (distinct)
        if (main.Length < config.MainNumbersCount)
        {
            var fill = RandomDistinct(1, config.MainNumbersRange, main, config.MainNumbersCount - main.Length, rng);
            main = main.Concat(fill).ToImmutableArray();
        }

        // 4) Bonus numbers (random, distinct from main)
        var bonus = config.BonusNumbersCount > 0
            ? RandomDistinct(1, config.BonusNumbersRange, main, config.BonusNumbersCount, rng)
            : ImmutableArray<int>.Empty;

        // 5) Confidence: same overlap ratio as original implementation
        var confidence = CalculateRepeatingNumbersConfidence(history, main.ToList());

        return new PredictionResult(
            config.LotteryId,
            main,
            bonus,
            confidence,
            PredictionAlgorithmKeys.RepeatingNumbers);
    }

    // ---------- helpers (PURE) ----------

    private static List<int> GetRecentNumbers(IReadOnlyList<HistoricalDraw> history, int take)
        => history.Take(take).SelectMany(d => d.WinningNumbers).ToList();

    private static Dictionary<int, int> IdentifyRepeatingNumbers(List<int> numbers)
    {
        var freq = new Dictionary<int, int>();
        foreach (var n in numbers)
            freq[n] = freq.GetValueOrDefault(n) + 1;

        return freq.Where(kv => kv.Value > 1)
            .OrderByDescending(kv => kv.Value)
            .ToDictionary(kv => kv.Key, kv => kv.Value);
    }

    private static List<int> GeneratePredictionsFromRepeatingNumbers(Dictionary<int, int> repeating, int count)
        => repeating.Keys.Take(count).ToList();

    private static double CalculateRepeatingNumbersConfidence(
        IReadOnlyCollection<HistoricalDraw> draws,
        List<int> predicted)
    {
        if (draws.Count == 0 || predicted.Count == 0) return 0d;

        var matches = 0;
        foreach (var d in draws)
            matches += d.WinningNumbers.Intersect(predicted).Count();

        return (double)matches / (draws.Count * predicted.Count);
    }

    private static ImmutableArray<int> RandomDistinct(
        int minInclusive,
        int maxInclusive,
        ImmutableArray<int> exclude,
        int count,
        Random rng)
    {
        if (count <= 0) return ImmutableArray<int>.Empty;

        var candidates = Enumerable.Range(minInclusive, maxInclusive - minInclusive + 1)
            .Except(exclude);
        return candidates.OrderBy(_ => rng.Next())
            .Take(count)
            .ToImmutableArray();
    }
}