using System.Collections.Immutable;
using JackpotPlot.Domain.Models;

namespace JackpotPlot.Domain.Predictions.Helpers;

public sealed class QuadrantAnalysisAlgorithmHelpers
{
    public static List<(int start, int end)> DivideIntoQuadrants(int maxRange, int quadrantCount)
    {
        var quads = new List<(int start, int end)>(quadrantCount);
        var size = maxRange / quadrantCount;
        for (var i = 0; i < quadrantCount; i++)
        {
            var start = i * size + 1;
            var end = (i == quadrantCount - 1) ? maxRange : (i + 1) * size;
            quads.Add((start, end));
        }
        return quads;
    }

    public static Dictionary<(int start, int end), int> AnalyzeQuadrantFrequencies(
        IEnumerable<HistoricalDraw> historicalDraws,
        List<(int start, int end)> quadrants)
    {
        var freq = quadrants.ToDictionary(q => q, _ => 0);
        foreach (var draw in historicalDraws)
        {
            foreach (var n in draw.WinningNumbers)
            {
                var q = quadrants.FirstOrDefault(x => n >= x.start && n <= x.end);
                if (q != default) freq[q]++;
            }
        }
        return freq;
    }

    public static ImmutableArray<int> GenerateNumbersFromQuadrants(
        List<(int start, int end)> quadrants,
        Dictionary<(int start, int end), int> frequencies,
        int count,
        Random rng)
    {
        if (count <= 0) return ImmutableArray<int>.Empty;

        var selected = new List<int>(count);
        var total = Math.Max(0, frequencies.Values.Sum());
        var proportion = new Dictionary<(int start, int end), int>();

        if (total == 0)
        {
            // even split if no history
            var per = count / quadrants.Count;
            var rem = count % quadrants.Count;
            for (int i = 0; i < quadrants.Count; i++)
                proportion[quadrants[i]] = per + (i < rem ? 1 : 0);
        }
        else
        {
            foreach (var q in quadrants)
            {
                var share = (double)frequencies[q] / total;
                proportion[q] = (int)Math.Round(share * count, MidpointRounding.AwayFromZero);
            }

            // normalize to exact 'count'
            var diff = proportion.Values.Sum() - count;
            if (diff != 0)
            {
                var keys = proportion.OrderByDescending(kv => kv.Value).Select(kv => kv.Key).ToList();
                var sign = Math.Sign(diff);
                diff = Math.Abs(diff);
                var i = 0;
                while (diff-- > 0 && keys.Count > 0)
                {
                    proportion[keys[i]] -= sign; // subtract if too many; add if too few
                    i = (i + 1) % keys.Count;
                }
            }
        }

        foreach (var q in quadrants)
        {
            var need = Math.Max(0, proportion[q]);
            if (need == 0) continue;

            var pool = Enumerable.Range(q.start, q.end - q.start + 1)
                .Except(selected);
            selected.AddRange(pool.OrderBy(_ => rng.Next()).Take(need));

            if (selected.Count >= count) break;
        }

        // fill if still short
        if (selected.Count < count)
        {
            var min = quadrants.First().start;
            var max = quadrants.Last().end;
            var fill = RandomDistinct(min, max, selected.ToImmutableArray(), count - selected.Count, rng);
            selected.AddRange(fill);
        }

        return selected
            .OrderBy(_ => rng.Next())
            .Take(count)
            .ToImmutableArray();
    }

    public static double CalculateQuadrantConfidence(
        IEnumerable<HistoricalDraw> historicalDraws,
        List<int> predictedNumbers,
        List<(int start, int end)> quadrants)
    {
        var histCounts = AnalyzeQuadrantFrequencies(historicalDraws, quadrants);
        var predCounts = quadrants.ToDictionary(q => q, _ => 0);
        foreach (var n in predictedNumbers)
        {
            var q = quadrants.FirstOrDefault(x => n >= x.start && n <= x.end);
            if (q != default) predCounts[q]++;
        }

        double totalDiff = 0;
        foreach (var q in quadrants)
            totalDiff += Math.Abs(predCounts[q] - histCounts[q]);

        return 1.0 / (1.0 + totalDiff);
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
        return candidates.OrderBy(_ => rng.Next())
            .Take(count)
            .ToImmutableArray();
    }
}