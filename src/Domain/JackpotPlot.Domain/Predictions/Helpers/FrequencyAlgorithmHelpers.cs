using JackpotPlot.Domain.Models;

namespace JackpotPlot.Domain.Predictions.Helpers;

public static class FrequencyAlgorithmHelpers
{
    public static List<(int Number, int Frequency)> CalcFreq(IEnumerable<int> numbers, int range)
    {
        var freq = new int[range + 1];
        foreach (var n in numbers) freq[n]++;
        return Enumerable.Range(1, range).Select(n => (n, freq[n])).ToList();
    }

    public static double FrequencyConfidence(IEnumerable<HistoricalDraw> history, List<int> predicted)
    {
        var historicalDraws = history as HistoricalDraw[] ?? history.ToArray();

        var total = historicalDraws.Count();
        if (total == 0 || predicted.Count == 0) return 0;
        var matches = historicalDraws.Sum(d => d.WinningNumbers.Intersect(predicted).Count());
        return (double)matches / (total * predicted.Count);
    }
}