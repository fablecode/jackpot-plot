using System.Collections.Immutable;
using JackpotPlot.Domain.Models;

namespace JackpotPlot.Domain.Predictions.Helpers;

public sealed class GapAnalysisAlgorithmHelpers
{
    public static Dictionary<int, int> AnalyzeGaps(IEnumerable<HistoricalDraw> draws)
    {
        var freq = new Dictionary<int, int>();
        foreach (var d in draws)
        {
            var numbers = d.WinningNumbers.OrderBy(n => n).ToList();
            for (var i = 1; i < numbers.Count; i++)
            {
                var gap = numbers[i] - numbers[i - 1];
                if (gap <= 0) continue;
                freq[gap] = freq.GetValueOrDefault(gap) + 1;
            }
        }
        return freq;
    }

    public static List<int> SelectGaps(Dictionary<int, int> gapFrequencies, int count)
        => gapFrequencies
            .OrderByDescending(kv => kv.Value)
            .ThenBy(kv => kv.Key)
            .Take(count)
            .Select(kv => kv.Key)
            .ToList();

    public static ImmutableArray<int> GenerateNumbersFromGaps(
        int maxRange,
        List<int> selectedGaps,
        int targetCount,
        Random rng)
    {
        if (targetCount <= 0) return ImmutableArray<int>.Empty;

        // pick a start in the lower half to reduce overflow when adding gaps
        var start = Math.Max(1, rng.Next(1, Math.Max(2, maxRange / 2)));
        var numbers = new List<int> { start };

        foreach (var gap in selectedGaps)
        {
            if (numbers.Count >= targetCount) break;

            var next = numbers[^1] + gap;
            if (next > maxRange) break;        // out of range → stop using gaps
            if (next <= 0) continue;
            if (!numbers.Contains(next)) numbers.Add(next);
        }

        // fill if we still don't have enough numbers
        if (numbers.Count < targetCount)
        {
            var fill = RandomDistinct(1, maxRange, numbers.ToImmutableArray(),
                targetCount - numbers.Count, rng);
            numbers.AddRange(fill);
        }

        // keep stable order
        return numbers.OrderBy(n => n).ToImmutableArray();
    }

    public static double CalculateGapAnalysisConfidence(
        IEnumerable<HistoricalDraw> history, List<int> predicted)
    {
        var hist = history.ToList();
        if (hist.Count == 0 || predicted.Count < 2) return 0;

        int correct = 0, total = 0;

        var predictedGaps = GetGaps(predicted);
        foreach (var draw in hist)
        {
            var actualGaps = GetGaps(draw.WinningNumbers);
            correct += actualGaps.Intersect(predictedGaps).Count();
            total += actualGaps.Count;
        }

        return total == 0 ? 0 : (double)correct / total;
    }

    private static List<int> GetGaps(IEnumerable<int> numbers)
    {
        var sorted = numbers.OrderBy(n => n).ToList();
        var gaps = new List<int>();
        for (var i = 1; i < sorted.Count; i++)
        {
            var gap = sorted[i] - sorted[i - 1];
            if (gap > 0) gaps.Add(gap);
        }
        return gaps;
    }

    public static ImmutableArray<int> RandomDistinct(
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