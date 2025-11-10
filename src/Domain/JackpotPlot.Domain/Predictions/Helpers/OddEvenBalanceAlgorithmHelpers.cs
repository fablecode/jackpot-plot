using System.Collections.Immutable;
using JackpotPlot.Domain.Models;

namespace JackpotPlot.Domain.Predictions.Helpers;

public sealed class OddEvenBalanceAlgorithmHelpers
{
    public static (double oddRatio, double evenRatio) CalculateOddEvenRatio(IEnumerable<HistoricalDraw> historicalDraws)
    {
        int odd = 0, even = 0;
        foreach (var d in historicalDraws)
        {
            foreach (var n in d.WinningNumbers)
            {
                if ((n & 1) == 1) odd++;
                else even++;
            }
        }

        var total = odd + even;
        return total == 0 ? (0.5, 0.5) : (odd / (double)total, even / (double)total);
    }

    public static ImmutableArray<int> GenerateNumbers(
        int minInclusive,
        int maxInclusive,
        Func<int, bool> predicate,
        int count,
        Random rng)
    {
        if (count <= 0) return ImmutableArray<int>.Empty;

        var pool = Enumerable.Range(minInclusive, maxInclusive - minInclusive + 1)
            .Where(predicate);

        return pool.OrderBy(_ => rng.Next())
            .Take(count)
            .ToImmutableArray();
    }

    public static double CalculateOddEvenBalanceConfidence(
        IReadOnlyCollection<HistoricalDraw> historicalDraws,
        List<int> predictedNumbers)
    {
        if (historicalDraws.Count == 0 || predictedNumbers.Count == 0) return 0d;

        var predictedOdd = predictedNumbers.Count(n => (n & 1) == 1);
        var predictedEven = predictedNumbers.Count - predictedOdd;

        var matches = 0;
        foreach (var draw in historicalDraws)
        {
            var odd = draw.WinningNumbers.Count(n => (n & 1) == 1);
            var even = draw.WinningNumbers.Count - odd;
            if (odd == predictedOdd && even == predictedEven)
                matches++;
        }

        return matches / (double)historicalDraws.Count;
    }
}