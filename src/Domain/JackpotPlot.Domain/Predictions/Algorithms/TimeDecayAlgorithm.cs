using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.ValueObjects;
using System.Collections.Immutable;
using JackpotPlot.Domain.Predictions.Helpers;
using JackpotPlot.Primitives.Algorithms;

namespace JackpotPlot.Domain.Predictions.Algorithms;

[PredictionAlgorithmDescription(PredictionAlgorithmKeys.TimeDecay, "Weighs recent draws more heavily than older ones, using a decay function so that the most current trends have greater influence on the prediction.")]
public sealed class TimeDecayAlgorithm : IPredictionAlgorithm
{
    private readonly double _decayFactor;

    public TimeDecayAlgorithm(double decayFactor = 0.9)
    {
        // clamp to (0,1]; lower means faster decay
        _decayFactor = Math.Clamp(decayFactor, 0.0001, 1.0);
    }

    public PredictionResult Predict(
        LotteryConfigurationDomain config,
        IReadOnlyList<HistoricalDraw> history,
        Random rng)
    {
        if (history.Count == 0)
        {
            return new PredictionResult(
                config.LotteryId,
                ImmutableArray<int>.Empty,
                ImmutableArray<int>.Empty,
                0d,
                PredictionAlgorithmKeys.TimeDecay);
        }

        // assign decayed weights: newest draw gets highest weight
        var weightsById = TimeDecayAlgorithmHelpers.AssignTimeDecayWeights(history, _decayFactor);

        // build weighted frequency over [1..MainNumbersRange]
        var weighted = TimeDecayAlgorithmHelpers.CalculateWeightedFrequencies(history, weightsById, config.MainNumbersRange);

        // sample main numbers proportionally to weight (without replacement)
        var main = TimeDecayAlgorithmHelpers.WeightedSampleDistinct(weighted, config.MainNumbersCount, rng).ToImmutableArray();

        // bonus numbers random & distinct from main
        var bonus = config.BonusNumbersCount > 0
            ? TimeDecayAlgorithmHelpers.RandomDistinct(1, config.BonusNumbersRange, main, config.BonusNumbersCount, rng)
            : ImmutableArray<int>.Empty;

        // confidence: how much we match the last 10 draws (like original)
        var confidence = TimeDecayAlgorithmHelpers.TimeDecayConfidence(history, main.ToList());

        return new PredictionResult(
            config.LotteryId, main, bonus, confidence, PredictionAlgorithmKeys.TimeDecay);
    }
}