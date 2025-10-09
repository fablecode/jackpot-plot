using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.ValueObjects;
using System.Collections.Immutable;
using JackpotPlot.Primitives.Algorithms;

namespace JackpotPlot.Domain.Predictions.Algorithms;

[PredictionAlgorithmDescription(PredictionAlgorithmKeys.PatternMatching, "Identifies recurring patterns (such as odd/even or high/low sequences) in historical data and uses these patterns as templates for future predictions.")]
public sealed class PatternMatchingAlgorithm : IPredictionAlgorithm
{
    public PredictionResult Predict(
        LotteryConfigurationDomain config,
        IReadOnlyList<HistoricalDraw> history,
        Random rng)
    {
        // No history → return empty (or you could random-fill if you prefer)
        if (history.Count == 0)
        {
            return new PredictionResult(
                config.LotteryId,
                ImmutableArray<int>.Empty,
                ImmutableArray<int>.Empty,
                0d,
                PredictionAlgorithmKeys.PatternMatching);
        }

        // 1) Build pattern frequencies from history (uses O/E + H/L like original)
        var patterns = AnalyzePatterns(history, config);

        // 2) Select most frequent pattern (fallback: empty pattern -> random)
        var selectedPattern = SelectMostFrequentPattern(patterns);

        // 3) Generate main numbers matching the pattern tokens
        ImmutableArray<int> main;
        if (!string.IsNullOrEmpty(selectedPattern))
        {
            main = GenerateNumbersFromPattern(selectedPattern, config.MainNumbersRange, rng);
        }
        else
        {
            main = RandomDistinct(1, config.MainNumbersRange, ImmutableArray<int>.Empty, config.MainNumbersCount, rng);
        }

        // 4) Bonus numbers (random, distinct from main)
        var bonus = config.BonusNumbersCount > 0
            ? RandomDistinct(1, config.BonusNumbersRange, main, config.BonusNumbersCount, rng)
            : ImmutableArray<int>.Empty;

        // 5) Confidence: share of historical draws whose full pattern equals predicted pattern
        var confidence = CalculatePatternMatchingConfidence(history, selectedPattern, config);

        return new PredictionResult(
            config.LotteryId,
            main,
            bonus,
            confidence,
            PredictionAlgorithmKeys.PatternMatching);
    }

    // ---------- helpers (PURE) ----------

    private static Dictionary<string, int> AnalyzePatterns(
        IEnumerable<HistoricalDraw> historicalDraws,
        LotteryConfigurationDomain config)
    {
        var patterns = new Dictionary<string, int>(StringComparer.Ordinal);
        var half = config.MainNumbersRange / 2;

        foreach (var draw in historicalDraws)
        {
            // only consider draws with the expected count
            if (draw.WinningNumbers.Count != config.MainNumbersCount) continue;

            // Build pattern tokens per position: "E/H", "O/L", etc (e.g., "EL,OH,..." like original)
            var pattern = string.Join(",",
                draw.WinningNumbers.Select(n =>
                {
                    var oddEven = (n % 2 == 0) ? "E" : "O";
                    var highLow = (n <= half) ? "L" : "H";
                    return $"{oddEven}{highLow}";
                }));

            if (!patterns.TryAdd(pattern, 1))
                patterns[pattern]++;
        }

        return patterns;
    }

    private static string SelectMostFrequentPattern(Dictionary<string, int> patterns)
        => patterns.Count == 0
            ? string.Empty
            : patterns.OrderByDescending(p => p.Value).First().Key;

    private static ImmutableArray<int> GenerateNumbersFromPattern(
        string pattern,
        int maxRange,
        Random rng)
    {
        if (string.IsNullOrWhiteSpace(pattern)) return ImmutableArray<int>.Empty;

        var tokens = pattern.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var used = new HashSet<int>();
        var half = maxRange / 2;
        var result = new List<int>(tokens.Length);

        foreach (var token in tokens)
        {
            // Build a candidate pool for this token
            IEnumerable<int> pool = Enumerable.Range(1, maxRange);

            // Odd/Even constraint
            if (token.Contains('E')) pool = pool.Where(n => (n & 1) == 0);
            if (token.Contains('O')) pool = pool.Where(n => (n & 1) == 1);

            // High/Low constraint
            if (token.Contains('H')) pool = pool.Where(n => n > half);
            if (token.Contains('L')) pool = pool.Where(n => n <= half);

            // Exclude already used
            pool = pool.Except(used);

            // If pool exhausted (edge-case), fallback to any remaining unused number
            var pick = pool.OrderBy(_ => rng.Next()).FirstOrDefault();
            if (pick == 0)
            {
                var fallback = Enumerable.Range(1, maxRange).Except(used);
                pick = fallback.OrderBy(_ => rng.Next()).First();
            }

            used.Add(pick);
            result.Add(pick);
        }

        return result.OrderBy(_ => rng.Next()).ToImmutableArray();
    }

    private static double CalculatePatternMatchingConfidence(
        IEnumerable<HistoricalDraw> historicalDraws,
        string predictedPattern,
        LotteryConfigurationDomain config)
    {
        if (string.IsNullOrEmpty(predictedPattern)) return 0d;

        var half = config.MainNumbersRange / 2;
        var matchCount = 0;
        var total = 0;

        foreach (var draw in historicalDraws)
        {
            if (draw.WinningNumbers.Count != config.MainNumbersCount) continue;

            total++;
            var actualPattern = string.Join(",",
                draw.WinningNumbers.Select(n =>
                {
                    var oddEven = (n % 2 == 0) ? "E" : "O";
                    var highLow = (n <= half) ? "L" : "H";
                    return $"{oddEven}{highLow}";
                }));

            if (actualPattern.Equals(predictedPattern, StringComparison.Ordinal))
                matchCount++;
        }

        return total == 0 ? 0d : (double)matchCount / total;
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

        return candidates
            .OrderBy(_ => rng.Next())
            .Take(count)
            .ToImmutableArray();
    }
}