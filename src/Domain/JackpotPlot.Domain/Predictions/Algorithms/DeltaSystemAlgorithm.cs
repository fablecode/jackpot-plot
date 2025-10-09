using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.ValueObjects;
using System.Collections.Immutable;
using JackpotPlot.Primitives.Algorithms;

namespace JackpotPlot.Domain.Predictions.Algorithms;

[PredictionAlgorithmDescription(PredictionAlgorithmKeys.DeltaSystem, "Examines the differences (deltas) between consecutive numbers in past draws and uses these patterns to generate predictions.")]
public sealed class DeltaSystemAlgorithm : IPredictionAlgorithm
{
    public PredictionResult Predict(
        LotteryConfigurationDomain config,
        IReadOnlyList<HistoricalDraw> history,
        Random rng)
    {
        // 1) collect all deltas from history
        var allDeltas = CalculateDeltas(history);

        // if we’ve got no history, just random-fill
        if (allDeltas.Count == 0)
        {
            var fallback = RandomDistinct(1, config.MainNumbersRange, [], config.MainNumbersCount, rng);
            var bonusFb = config.BonusNumbersCount > 0
                ? RandomDistinct(1, config.BonusNumbersRange, fallback, config.BonusNumbersCount, rng)
                : ImmutableArray<int>.Empty;

            return new PredictionResult(
                config.LotteryId, fallback, bonusFb, 0, PredictionAlgorithmKeys.DeltaSystem);
        }

        // 2) pick most frequent deltas; for a line of N numbers we need up to N-1 deltas
        var neededDeltas = Math.Max(0, config.MainNumbersCount - 1);
        var frequentDeltas = GetFrequentDeltas(allDeltas, neededDeltas);

        // 3) generate main numbers from the deltas (start in lower half; respect range)
        var main = GenerateNumbersFromDeltas(
            frequentDeltas, config.MainNumbersRange, config.MainNumbersCount, rng);

        // 4) bonus numbers (random, distinct from main)
        var bonus = config.BonusNumbersCount > 0
            ? RandomDistinct(1, config.BonusNumbersRange, main, config.BonusNumbersCount, rng)
            : ImmutableArray<int>.Empty;

        // 5) confidence (how many predicted deltas match typical historical deltas)
        var confidence = CalculateDeltaSystemConfidence(history, main.ToList());

        return new PredictionResult(
            config.LotteryId, main, bonus, confidence, PredictionAlgorithmKeys.DeltaSystem);
    }

    // ---------- helpers (pure) ----------

    private static List<int> CalculateDeltas(IReadOnlyList<HistoricalDraw> historicalDraws)
    {
        var deltas = new List<int>();
        foreach (var draw in historicalDraws)
        {
            var nums = draw.WinningNumbers.OrderBy(n => n).ToList();
            for (int i = 1; i < nums.Count; i++)
                deltas.Add(nums[i] - nums[i - 1]);
        }
        return deltas;
    }

    private static List<int> GetFrequentDeltas(List<int> deltas, int take)
    {
        if (take == 0) return new List<int>();
        return deltas
            .GroupBy(d => d)
            .OrderByDescending(g => g.Count())
            .ThenBy(g => g.Key)
            .Take(take)
            .Select(g => g.Key)
            .ToList();
    }

    private static ImmutableArray<int> GenerateNumbersFromDeltas(
        List<int> deltas, int maxRange, int targetCount, Random rng)
    {
        if (targetCount <= 0)
            return ImmutableArray<int>.Empty;

        // start in lower half to reduce overflow from adding deltas
        var start = Math.Max(1, rng.Next(1, Math.Max(2, (maxRange / 2))));
        var numbers = new List<int> { start };

        foreach (var delta in deltas)
        {
            if (numbers.Count >= targetCount) break;

            var next = numbers[^1] + delta;
            if (next > maxRange) break;
            if (next <= 0) continue;

            if (!numbers.Contains(next))
                numbers.Add(next);
        }

        // if we ran out of deltas or exceeded bounds, fill randomly (distinct)
        if (numbers.Count < targetCount)
        {
            var fill = RandomDistinct(1, maxRange, numbers.ToImmutableArray(),
                targetCount - numbers.Count, rng);
            numbers.AddRange(fill);
        }

        // keep stable order
        return numbers.OrderBy(n => n).ToImmutableArray();
    }

    private static double CalculateDeltaSystemConfidence(
        IReadOnlyCollection<HistoricalDraw> historicalDraws, List<int> predictedNumbers)
    {
        if (historicalDraws.Count == 0 || predictedNumbers.Count < 2)
            return 0;

        int correct = 0, total = 0;

        var predictedDeltas = CalculateDeltas(predictedNumbers);

        foreach (var draw in historicalDraws)
        {
            var actualDeltas = CalculateDeltas(draw.WinningNumbers);
            correct += actualDeltas.Intersect(predictedDeltas).Count();
            total += actualDeltas.Count;
        }

        return total == 0 ? 0 : (double)correct / total;
    }

    // overload for lists
    private static List<int> CalculateDeltas(List<int> numbers)
    {
        var deltas = new List<int>();
        numbers.Sort();
        for (int i = 1; i < numbers.Count; i++)
            deltas.Add(numbers[i] - numbers[i - 1]);
        return deltas;
    }

    private static ImmutableArray<int> RandomDistinct(
        int minInclusive, int maxInclusive, ImmutableArray<int> exclude, int count, Random rng)
    {
        if (count <= 0) return ImmutableArray<int>.Empty;

        var candidates = Enumerable.Range(minInclusive, maxInclusive - minInclusive + 1)
            .Except(exclude);
        return candidates.OrderBy(_ => rng.Next()).Take(count).ToImmutableArray();
    }
}