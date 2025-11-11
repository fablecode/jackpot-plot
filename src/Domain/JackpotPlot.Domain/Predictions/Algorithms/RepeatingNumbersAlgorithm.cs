using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.ValueObjects;
using System.Collections.Immutable;
using JackpotPlot.Domain.Predictions.Helpers;
using JackpotPlot.Primitives.Algorithms;

namespace JackpotPlot.Domain.Predictions.Algorithms;

[PredictionAlgorithmDescription(PredictionAlgorithmKeys.RepeatingNumbers, "Focuses on numbers that have appeared frequently in recent draws, under the assumption that such numbers might continue to repeat.")]
public sealed class RepeatingNumbersAlgorithm : IPredictionAlgorithm
{
    private readonly int _recentDrawsToConsider;

    public RepeatingNumbersAlgorithm(int recentDrawsToConsider = 10)
    {
        _recentDrawsToConsider = Math.Max(1, recentDrawsToConsider);
    }

    public PredictionResult Predict(
        LotteryConfigurationDomain config,
        IReadOnlyList<HistoricalDraw> history,
        Random rng)
    {
        // If no history, return empty (or you could random-fill if desired)
        if (history.Count == 0)
        {
            return new PredictionResult(
                config.LotteryId,
                ImmutableArray<int>.Empty,
                ImmutableArray<int>.Empty,
                0d,
                PredictionAlgorithmKeys.RepeatingNumbers);
        }

        // 1) Collect numbers from the N most recent draws
        // NOTE: assumes 'history' is already ordered newest-first like the original Take(N) usage.
        var recent = RepeatingNumbersAlgorithmHelpers.GetRecentNumbers(history, _recentDrawsToConsider);

        // 2) Identify repeating numbers (appear > 1 time), ordered by frequency desc
        var repeating = RepeatingNumbersAlgorithmHelpers.IdentifyRepeatingNumbers(recent);

        // 3) Take top K by frequency for main numbers (like original)
        var main = RepeatingNumbersAlgorithmHelpers.GeneratePredictionsFromRepeatingNumbers(repeating, config.MainNumbersCount)
            .ToImmutableArray();

        // If fewer than needed (e.g., not enough repeats), top up randomly (distinct)
        if (main.Length < config.MainNumbersCount)
        {
            var fill = RepeatingNumbersAlgorithmHelpers.RandomDistinct(1, config.MainNumbersRange, main, config.MainNumbersCount - main.Length, rng);
            main = main.Concat(fill).ToImmutableArray();
        }

        // 4) Bonus numbers (random, distinct from main)
        var bonus = config.BonusNumbersCount > 0
            ? RepeatingNumbersAlgorithmHelpers.RandomDistinct(1, config.BonusNumbersRange, main, config.BonusNumbersCount, rng)
            : ImmutableArray<int>.Empty;

        // 5) Confidence: same overlap ratio as original implementation
        var confidence = RepeatingNumbersAlgorithmHelpers.CalculateRepeatingNumbersConfidence(history, main.ToList());

        return new PredictionResult(
            config.LotteryId,
            main,
            bonus,
            confidence,
            PredictionAlgorithmKeys.RepeatingNumbers);
    }
}