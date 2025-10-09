using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.ValueObjects;
using System.Collections.Immutable;
using JackpotPlot.Primitives.Algorithms;

namespace JackpotPlot.Domain.Predictions.Algorithms;

[PredictionAlgorithmDescription(PredictionAlgorithmKeys.SeasonalPatterns, "Analyzes seasonal or temporal trends in historical draws (by month, season, etc.) to predict numbers that are more likely to appear during the current season.")]
public sealed class SeasonalPatternsAlgorithm : IPredictionAlgorithm
{
    public PredictionResult Predict(
        LotteryConfigurationDomain config,
        IReadOnlyList<HistoricalDraw> history,
        Random rng)
    {
        // no history → empty (or fallback to random if you prefer)
        if (history.Count == 0)
        {
            return new PredictionResult(
                config.LotteryId,
                ImmutableArray<int>.Empty,
                ImmutableArray<int>.Empty,
                0d,
                PredictionAlgorithmKeys.SeasonalPatterns);
        }

        // 1) Determine current season (by month)
        var currentSeason = GetSeason(DateTime.UtcNow);

        // 2) Build seasonal frequency table over [1..MainNumbersRange]
        var seasonalFreq = AnalyzeSeasonalFrequencies(history, currentSeason, config.MainNumbersRange);

        // 3) Choose main numbers by seasonal frequency (break ties with rng)
        var main = GenerateFromSeasonalFrequencies(seasonalFreq, config.MainNumbersCount, rng)
            .ToImmutableArray();

        // If seasonal data is too sparse, top up randomly (distinct)
        if (main.Length < config.MainNumbersCount)
        {
            var fill = RandomDistinct(1, config.MainNumbersRange, main, config.MainNumbersCount - main.Length, rng);
            main = main.Concat(fill).ToImmutableArray();
        }

        // 4) Bonus numbers (random, distinct from main)
        var bonus = config.BonusNumbersCount > 0
            ? RandomDistinct(1, config.BonusNumbersRange, main, config.BonusNumbersCount, rng)
            : ImmutableArray<int>.Empty;

        // 5) Confidence: overlap rate with seasonal draws only
        var confidence = SeasonalConfidence(history, main.ToList(), currentSeason);

        return new PredictionResult(
            config.LotteryId,
            main,
            bonus,
            confidence,
            PredictionAlgorithmKeys.SeasonalPatterns);
    }

    // ---------- helpers (PURE) ----------

    private static string GetSeason(DateTime dateUtc)
    {
        var m = dateUtc.Month;
        return m switch
        {
            12 or 1 or 2 => "Winter",
            3 or 4 or 5 => "Spring",
            6 or 7 or 8 => "Summer",
            9 or 10 or 11 => "Fall",
            _ => "Unknown"
        };
    }

    private static string GetSeason(DateTimeOffset dto) => GetSeason(dto.UtcDateTime);

    private static Dictionary<int, int> AnalyzeSeasonalFrequencies(
        IEnumerable<HistoricalDraw> historicalDraws,
        string season,
        int numberRange)
    {
        var freq = Enumerable.Range(1, numberRange).ToDictionary(n => n, _ => 0);

        foreach (var draw in historicalDraws)
        {
            if (GetSeason(draw.DrawDate) != season) continue;
            foreach (var n in draw.WinningNumbers)
                if (n >= 1 && n <= numberRange) freq[n]++;
        }

        return freq;
    }

    private static ImmutableArray<int> GenerateFromSeasonalFrequencies(
        Dictionary<int, int> seasonalFreq,
        int take,
        Random rng)
    {
        if (take <= 0) return ImmutableArray<int>.Empty;

        return seasonalFreq
            .OrderByDescending(kv => kv.Value)
            .ThenBy(_ => rng.Next())      // tie-breaker for equal frequencies
            .Select(kv => kv.Key)
            .Take(take)
            .ToImmutableArray();
    }

    private static double SeasonalConfidence(
        IEnumerable<HistoricalDraw> historicalDraws,
        List<int> predictedNumbers,
        string season)
    {
        var seasonal = historicalDraws.Where(d => GetSeason(d.DrawDate) == season).ToList();
        if (seasonal.Count == 0 || predictedNumbers.Count == 0) return 0d;

        var matches = 0;
        foreach (var d in seasonal)
            matches += d.WinningNumbers.Intersect(predictedNumbers).Count();

        return (double)matches / (seasonal.Count * predictedNumbers.Count);
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