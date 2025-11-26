using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.ValueObjects;
using System.Collections.Immutable;
using JackpotPlot.Domain.Predictions.Helpers;
using JackpotPlot.Primitives.Algorithms;

namespace JackpotPlot.Domain.Predictions.Algorithms;

[PredictionAlgorithmDescription(PredictionAlgorithmKeys.SymmetryAnalysis, "Evaluates the balance between high/low and odd/even numbers in historical draws to produce predictions that maintain a symmetric or balanced distribution reflective of past trends.")]
public sealed class SymmetryAnalysisAlgorithm : IPredictionAlgorithm
{
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
                PredictionAlgorithmKeys.SymmetryAnalysis);
        }

        // analyze historical symmetry (high/low + odd/even)
        var metrics = SymmetryAnalysisAlgorithmHelpers.AnalyzeSymmetryMetrics(history, config.MainNumbersRange);

        // generate main numbers matching the symmetry ratios
        var main = SymmetryAnalysisAlgorithmHelpers.GenerateSymmetricNumbers(metrics, config.MainNumbersCount, config.MainNumbersRange, rng)
            .ToImmutableArray();

        // bonus numbers (random & distinct from main)
        var bonus = config.BonusNumbersCount > 0
            ? SymmetryAnalysisAlgorithmHelpers.RandomDistinct(1, config.BonusNumbersRange, main, config.BonusNumbersCount, rng)
            : ImmutableArray<int>.Empty;

        // confidence: how close our predicted ratios are to historical ratios
        var confidence = SymmetryAnalysisAlgorithmHelpers.CalculateSymmetryConfidence(main, metrics, config.MainNumbersRange);

        return new PredictionResult(
            config.LotteryId, main, bonus, confidence, PredictionAlgorithmKeys.SymmetryAnalysis);
    }
}