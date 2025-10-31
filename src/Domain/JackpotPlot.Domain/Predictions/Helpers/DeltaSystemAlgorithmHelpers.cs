using System.Collections.Immutable;
using JackpotPlot.Domain.Models;

namespace JackpotPlot.Domain.Predictions.Helpers;

public static class DeltaSystemAlgorithmHelpers
{
    public static List<int> CalculateDeltas(IReadOnlyList<HistoricalDraw> historicalDraws)
    {
        var deltas = new List<int>();
        foreach (var draw in historicalDraws)
        {
            var numbers = draw.WinningNumbers.OrderBy(n => n).ToList();
            for (var i = 1; i < numbers.Count; i++)
                deltas.Add(numbers[i] - numbers[i - 1]);
        }
        return deltas;
    }

    public static List<int> GetFrequentDeltas(List<int> deltas, int take)
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

    public static ImmutableArray<int> GenerateNumbersFromDeltas(
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

    public static double CalculateDeltaSystemConfidence(
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
    public static List<int> CalculateDeltas(List<int> numbers)
    {
        var deltas = new List<int>();
        numbers.Sort();
        for (int i = 1; i < numbers.Count; i++)
            deltas.Add(numbers[i] - numbers[i - 1]);
        return deltas;
    }

    public static ImmutableArray<int> RandomDistinct(
        int minInclusive, int maxInclusive, ImmutableArray<int> exclude, int count, Random rng)
    {
        if (count <= 0) return ImmutableArray<int>.Empty;

        var candidates = Enumerable.Range(minInclusive, maxInclusive - minInclusive + 1)
            .Except(exclude);
        return candidates.OrderBy(_ => rng.Next()).Take(count).ToImmutableArray();
    }
}