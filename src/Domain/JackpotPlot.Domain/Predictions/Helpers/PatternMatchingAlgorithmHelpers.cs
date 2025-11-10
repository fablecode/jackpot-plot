using System.Collections.Immutable;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;

namespace JackpotPlot.Domain.Predictions.Helpers;

public sealed class PatternMatchingAlgorithmHelpers
{
    private const char Even = 'E';
    private const char Odd = 'O';
    private const char Low = 'L';
    private const char High = 'H';

    public static Dictionary<string, int> AnalyzePatterns(
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
                    var oddEven = (n % 2 == 0) ? Even : Odd;
                    var highLow = (n <= half) ? Low : High;
                    return $"{oddEven}{highLow}";
                }));

            if (!patterns.TryAdd(pattern, 1))
                patterns[pattern]++;
        }

        return patterns;
    }

    public static string SelectMostFrequentPattern(Dictionary<string, int> patterns)
        => patterns.Count == 0
            ? string.Empty
            : patterns.OrderByDescending(p => p.Value).First().Key;

    public static ImmutableArray<int> GenerateNumbersFromPattern(
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
            if (token.Contains(Even)) pool = pool.Where(n => (n & 1) == 0);
            if (token.Contains(Odd)) pool = pool.Where(n => (n & 1) == 1);

            // High/Low constraint
            if (token.Contains(High)) pool = pool.Where(n => n > half);
            if (token.Contains(Low)) pool = pool.Where(n => n <= half);

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

    public static double CalculatePatternMatchingConfidence(
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
                    var oddEven = (n % 2 == 0) ? Even : Odd;
                    var highLow = (n <= half) ? Low : High;
                    return $"{oddEven}{highLow}";
                }));

            if (actualPattern.Equals(predictedPattern, StringComparison.Ordinal))
                matchCount++;
        }

        return total == 0 ? 0d : (double)matchCount / total;
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