using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.ValueObjects;
using System.Collections.Immutable;
using JackpotPlot.Primitives.Algorithms;

namespace JackpotPlot.Domain.Predictions.Algorithms;

[PredictionAlgorithmDescription(PredictionAlgorithmKeys.GroupSelection, "Splits the number range into predefined groups (e.g., low, medium, high) and selects numbers proportionally from each group based on historical frequency.")]
public sealed class GroupSelectionAlgorithm : IPredictionAlgorithm
{
    public PredictionResult Predict(
        LotteryConfigurationDomain config,
        IReadOnlyList<HistoricalDraw> history,
        Random rng)
    {
        // 1) make 3 groups (low/medium/high). tweak groupCount if you want more buckets.
        const int groupCount = 3;
        var groups = DivideIntoGroups(config.MainNumbersRange, groupCount);

        // 2) build frequency per group from history
        var groupFreq = AnalyzeGroupFrequencies(history, groups);

        // 3) pick main numbers proportionally to group frequency
        var main = GenerateNumbersFromGroups(groups, groupFreq, config.MainNumbersCount, rng)
            .ToImmutableArray();

        // 4) make bonus numbers (random, distinct from main)
        var bonus = config.BonusNumbersCount > 0
            ? RandomDistinct(1, config.BonusNumbersRange, main, config.BonusNumbersCount, rng)
            : ImmutableArray<int>.Empty;

        // 5) confidence: compare predicted group distribution to historical distribution
        var confidence = CalculateGroupConfidence(history, main, groups, groupFreq);

        return new PredictionResult(
            config.LotteryId,
            main,
            bonus,
            confidence,
            PredictionAlgorithmKeys.GroupSelection);
    }

    // ---------- helpers (PURE) ----------

    private static List<(int start, int end)> DivideIntoGroups(int numberRange, int groupCount)
    {
        var groups = new List<(int start, int end)>(groupCount);
        var baseSize = numberRange / groupCount;
        var remainder = numberRange % groupCount;

        var start = 1;
        for (int i = 0; i < groupCount; i++)
        {
            var size = baseSize + (i < remainder ? 1 : 0);
            var end = start + size - 1;
            groups.Add((start, end));
            start = end + 1;
        }

        return groups;
    }

    private static Dictionary<(int start, int end), int> AnalyzeGroupFrequencies(
        IEnumerable<HistoricalDraw> historicalDraws,
        List<(int start, int end)> groups)
    {
        var freq = groups.ToDictionary(g => g, _ => 0);
        foreach (var draw in historicalDraws)
        {
            foreach (var n in draw.WinningNumbers)
            {
                var g = groups.FirstOrDefault(x => n >= x.start && n <= x.end);
                if (g != default) freq[g]++;
            }
        }
        return freq;
    }

    private static ImmutableArray<int> GenerateNumbersFromGroups(
        List<(int start, int end)> groups,
        Dictionary<(int start, int end), int> groupFrequencies,
        int totalCount,
        Random rng)
    {
        if (totalCount <= 0) return ImmutableArray<int>.Empty;

        var selected = new List<int>(totalCount);
        var totalFreq = Math.Max(0, groupFrequencies.Values.Sum());

        // If no history, fall back to equal proportion per group.
        var proportional = new Dictionary<(int start, int end), int>();
        if (totalFreq == 0)
        {
            var perGroup = totalCount / groups.Count;
            var remainder = totalCount % groups.Count;
            for (int i = 0; i < groups.Count; i++)
                proportional[groups[i]] = perGroup + (i < remainder ? 1 : 0);
        }
        else
        {
            // initial proportional allocation (rounded)
            foreach (var g in groups)
            {
                var share = (double)groupFrequencies[g] / totalFreq;
                proportional[g] = (int)Math.Round(share * totalCount, MidpointRounding.AwayFromZero);
            }

            // normalize to exact totalCount (adjust largest groups first)
            var diff = proportional.Values.Sum() - totalCount;
            if (diff != 0)
            {
                var ordered = proportional.OrderByDescending(kv => kv.Value).Select(kv => kv.Key).ToList();
                var sign = Math.Sign(diff);
                diff = Math.Abs(diff);
                var idx = 0;
                while (diff-- > 0 && ordered.Count > 0)
                {
                    proportional[ordered[idx]] -= sign; // subtract if too many; add if too few
                    idx = (idx + 1) % ordered.Count;
                }
            }
        }

        // sample from each group's range without duplicates
        foreach (var g in groups)
        {
            var need = Math.Max(0, proportional[g]);
            if (need == 0) continue;

            var pool = Enumerable.Range(g.start, g.end - g.start + 1).Except(selected);
            selected.AddRange(pool.OrderBy(_ => rng.Next()).Take(need));

            if (selected.Count >= totalCount) break;
        }

        // If still short (due to small ranges/overlaps), fill uniformly across the whole range.
        if (selected.Count < totalCount)
        {
            var min = groups.First().start;
            var max = groups.Last().end;
            var fill = RandomDistinct(min, max, selected.ToImmutableArray(), totalCount - selected.Count, rng);
            selected.AddRange(fill);
        }

        return selected
            .OrderBy(_ => rng.Next())      // light shuffle
            .Take(totalCount)
            .ToImmutableArray();
    }

    private static double CalculateGroupConfidence(
        IEnumerable<HistoricalDraw> historicalDraws,
        ImmutableArray<int> predicted,
        List<(int start, int end)> groups,
        Dictionary<(int start, int end), int> historicalGroupCounts)
    {
        if (predicted.IsDefaultOrEmpty) return 0;

        // build predicted distribution
        var predictedCounts = groups.ToDictionary(g => g, _ => 0);
        foreach (var n in predicted)
        {
            var g = groups.FirstOrDefault(x => n >= x.start && n <= x.end);
            if (g != default) predictedCounts[g]++;
        }

        // compare distributions (L1 distance)
        double totalDiff = 0;
        foreach (var g in groups)
            totalDiff += Math.Abs(predictedCounts[g] - historicalGroupCounts.GetValueOrDefault(g));

        // Higher confidence if distributions are closer.
        return 1.0 / (1.0 + totalDiff);
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