using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.ValueObjects;
using System.Collections.Immutable;
using JackpotPlot.Primitives.Algorithms;

namespace JackpotPlot.Domain.Predictions.Algorithms;

[PredictionAlgorithmDescription(PredictionAlgorithmKeys.ConsecutiveNumbers, "Focuses on frequently occurring consecutive pairs or sequences, assuming that numbers appearing in a chain might appear together again.")]
public sealed class ConsecutiveNumbersAlgorithm : IPredictionAlgorithm
{
    public PredictionResult Predict(LotteryConfigurationDomain config, IReadOnlyList<HistoricalDraw> history, Random rng)
    {
        var consecutivePairs = FindFrequentConsecutivePairs(history);
        var selected = SelectConsecutiveNumbers(consecutivePairs, config.MainNumbersCount);

        var remaining = GenerateRandomNumbers(
            1, config.MainNumbersRange, selected, config.MainNumbersCount - selected.Count, rng);

        var predicted = selected.Concat(remaining).OrderBy(_ => rng.Next()).ToImmutableArray();

        var bonus = config.BonusNumbersCount > 0
            ? GenerateRandomNumbers(1, config.BonusNumbersRange, new List<int>(), config.BonusNumbersCount, rng)
            : ImmutableArray<int>.Empty;

        return new PredictionResult(
            config.LotteryId,
            predicted,
            bonus,
            CalculateConsecutiveNumbersConfidence(history, predicted.ToList()),
            PredictionAlgorithmKeys.ConsecutiveNumbers);
    }

    // ----- helpers (pure) -----
    private static Dictionary<(int, int), int> FindFrequentConsecutivePairs(IEnumerable<HistoricalDraw> draws)
    {
        var pairCounts = new Dictionary<(int, int), int>();
        foreach (var d in draws)
        {
            var nums = d.WinningNumbers.OrderBy(n => n).ToList();
            for (int i = 0; i < nums.Count - 1; i++)
            {
                if (nums[i + 1] == nums[i] + 1)
                    pairCounts[(nums[i], nums[i + 1])] = pairCounts.GetValueOrDefault((nums[i], nums[i + 1])) + 1;
            }
        }
        return pairCounts;
    }

    private static List<int> SelectConsecutiveNumbers(Dictionary<(int, int), int> pairs, int maxCount)
    {
        var set = new HashSet<int>();
        foreach (var p in pairs.OrderByDescending(p => p.Value).Take(maxCount))
        { set.Add(p.Key.Item1); set.Add(p.Key.Item2); }
        return set.Take(maxCount).ToList();
    }

    private static ImmutableArray<int> GenerateRandomNumbers(int min, int max, List<int> exclude, int count, Random rng)
        => Enumerable.Range(min, max - min + 1).Except(exclude).OrderBy(_ => rng.Next()).Take(count).ToImmutableArray();

    private static double CalculateConsecutiveNumbersConfidence(IEnumerable<HistoricalDraw> draws, List<int> predicted)
    {
        int correct = 0, total = 0;
        foreach (var d in draws)
        {
            var a = GetPairs(d.WinningNumbers);
            var b = GetPairs(predicted);
            correct += a.Intersect(b).Count();
            total += a.Count;
        }
        return total == 0 ? 0 : (double)correct / total;
    }

    private static List<(int, int)> GetPairs(List<int> nums)
    {
        var pairs = new List<(int, int)>();
        for (int i = 1; i < nums.Count; i++)
            if (nums[i] == nums[i - 1] + 1) pairs.Add((nums[i - 1], nums[i]));
        return pairs;
    }
}