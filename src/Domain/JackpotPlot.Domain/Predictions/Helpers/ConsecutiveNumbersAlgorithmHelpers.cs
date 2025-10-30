using System.Collections.Immutable;
using JackpotPlot.Domain.Models;

namespace JackpotPlot.Domain.Predictions.Helpers;

public static class ConsecutiveNumbersAlgorithmHelpers
{
    public static Dictionary<(int, int), int> FindFrequentConsecutivePairs(IEnumerable<HistoricalDraw> draws)
    {
        var pairCounts = new Dictionary<(int, int), int>();
        foreach (var d in draws)
        {
            var numbers = d.WinningNumbers.OrderBy(n => n).ToList();
            for (var i = 0; i < numbers.Count - 1; i++)
            {
                if (numbers[i + 1] == numbers[i] + 1)
                    pairCounts[(numbers[i], numbers[i + 1])] = pairCounts.GetValueOrDefault((numbers[i], numbers[i + 1])) + 1;
            }
        }
        return pairCounts;
    }

    public static List<int> SelectConsecutiveNumbers(Dictionary<(int, int), int> pairs, int maxCount)
    {
        var set = new HashSet<int>();
        foreach (var p in pairs.OrderByDescending(p => p.Value).Take(maxCount))
        { set.Add(p.Key.Item1); set.Add(p.Key.Item2); }
        return set.Take(maxCount).ToList();
    }

    public static ImmutableArray<int> GenerateRandomNumbers(int min, int max, List<int> exclude, int count, Random rng)
        => Enumerable.Range(min, max - min + 1).Except(exclude).OrderBy(_ => rng.Next()).Take(count).ToImmutableArray();

    public static double CalculateConsecutiveNumbersConfidence(IEnumerable<HistoricalDraw> draws, List<int> predicted)
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

    public static List<(int, int)> GetPairs(List<int> numbers)
    {
        var pairs = new List<(int, int)>();
        for (var i = 1; i < numbers.Count; i++)
            if (numbers[i] == numbers[i - 1] + 1) pairs.Add((numbers[i - 1], numbers[i]));
        return pairs;
    }
}