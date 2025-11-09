using System.Collections.Immutable;
using JackpotPlot.Domain.Models;

namespace JackpotPlot.Domain.Predictions.Helpers;

public sealed class NumberChainAlgorithmHelpers
{
    public static Dictionary<HashSet<int>, int> AnalyzeNumberChains(
        IEnumerable<HistoricalDraw> historicalDraws)
    {
        var chainFrequency = new Dictionary<HashSet<int>, int>(HashSet<int>.CreateSetComparer());

        foreach (var draw in historicalDraws)
        {
            var nums = draw.WinningNumbers;
            foreach (var chain in GenerateChains(nums))
            {
                if (!chainFrequency.ContainsKey(chain)) chainFrequency[chain] = 0;
                chainFrequency[chain]++;
            }
        }

        // order by frequency desc
        return chainFrequency
            .OrderByDescending(kv => kv.Value)
            .ToDictionary(kv => kv.Key, kv => kv.Value, HashSet<int>.CreateSetComparer());
    }

    public static List<int> GenerateNumbersFromChains(
        Dictionary<HashSet<int>, int> numberChains,
        int count,
        Random rng)
    {
        var selected = new HashSet<int>();
        foreach (var chain in numberChains.Keys)
        {
            foreach (var n in chain)
            {
                if (selected.Count < count) selected.Add(n);
                if (selected.Count >= count) break;
            }
            if (selected.Count >= count) break;
        }

        // light shuffle for variety
        return selected.OrderBy(_ => rng.Next()).ToList();
    }

    public static double CalculateChainConfidence(
        IEnumerable<HistoricalDraw> historicalDraws,
        List<int> predictedNumbers,
        Dictionary<HashSet<int>, int> numberChains)
    {
        if (predictedNumbers.Count == 0 || numberChains.Count == 0) return 0;

        int chainMatchCount = 0;
        int numberMatchCount = 0;
        var predictedSet = new HashSet<int>(predictedNumbers);

        // how many high-freq chains are subsets of the prediction
        foreach (var chain in numberChains.Keys)
            if (chain.IsSubsetOf(predictedSet)) chainMatchCount++;

        // how often predicted numbers appear historically
        foreach (var draw in historicalDraws)
            numberMatchCount += draw.WinningNumbers.Intersect(predictedSet).Count();

        var chainConfidence = (double)chainMatchCount / numberChains.Count;
        var numberConfidence = (double)numberMatchCount / (historicalDraws.Count() * predictedNumbers.Count);

        return (chainConfidence + numberConfidence) / 2.0;
    }

    private static IEnumerable<HashSet<int>> GenerateChains(List<int> numbers)
    {
        // all pairs & triplets
        for (int i = 0; i < numbers.Count; i++)
        {
            for (int j = i + 1; j < numbers.Count; j++)
            {
                yield return new HashSet<int> { numbers[i], numbers[j] };
                for (int k = j + 1; k < numbers.Count; k++)
                    yield return new HashSet<int> { numbers[i], numbers[j], numbers[k] };
            }
        }
    }

    public static ImmutableArray<int> GenerateRandomNumbers(
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