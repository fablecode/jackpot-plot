using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.ValueObjects;
using System.Collections.Immutable;
using JackpotPlot.Domain.Predictions.Helpers;
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
        var currentSeason = SeasonalPatternsAlgorithmHelpers.GetSeason(DateTime.UtcNow);

        // 2) Build seasonal frequency table over [1..MainNumbersRange]
        var seasonalFreq = SeasonalPatternsAlgorithmHelpers.AnalyzeSeasonalFrequencies(history, currentSeason, config.MainNumbersRange);

        // 3) Choose main numbers by seasonal frequency (break ties with rng)
        var main = SeasonalPatternsAlgorithmHelpers.GenerateFromSeasonalFrequencies(seasonalFreq, config.MainNumbersCount, rng)
            .ToImmutableArray();

        // If seasonal data is too sparse, top up randomly (distinct)
        if (main.Length < config.MainNumbersCount)
        {
            var fill = SeasonalPatternsAlgorithmHelpers.RandomDistinct(1, config.MainNumbersRange, main, config.MainNumbersCount - main.Length, rng);
            main = main.Concat(fill).ToImmutableArray();
        }

        // 4) Bonus numbers (random, distinct from main)
        var bonus = config.BonusNumbersCount > 0
            ? SeasonalPatternsAlgorithmHelpers.RandomDistinct(1, config.BonusNumbersRange, main, config.BonusNumbersCount, rng)
            : ImmutableArray<int>.Empty;

        // 5) Confidence: overlap rate with seasonal draws only
        var confidence = SeasonalPatternsAlgorithmHelpers.SeasonalConfidence(history, main.ToList(), currentSeason);

        return new PredictionResult(
            config.LotteryId,
            main,
            bonus,
            confidence,
            PredictionAlgorithmKeys.SeasonalPatterns);
    }
}