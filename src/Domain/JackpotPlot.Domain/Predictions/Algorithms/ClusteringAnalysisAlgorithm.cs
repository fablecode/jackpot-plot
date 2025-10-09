using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.ValueObjects;
using System.Collections.Immutable;
using JackpotPlot.Domain.Predictions.Helpers;
using JackpotPlot.Primitives.Algorithms;

namespace JackpotPlot.Domain.Predictions.Algorithms;

[PredictionAlgorithmDescription(PredictionAlgorithmKeys.ClusteringAnalysis, "Applies clustering techniques (like K-Means) to group numbers that appear together and selects predictions from the most prominent clusters.")]
public sealed class ClusteringAnalysisAlgorithm : IPredictionAlgorithm
{
    public PredictionResult Predict(LotteryConfigurationDomain config, IReadOnlyList<HistoricalDraw> history, Random random)
    {
        var co = ClusteringAnalysisPredictionStrategyHelpers
            .GenerateCoOccurrenceMatrix(history, config.MainNumbersRange);
        var clusters = ClusteringAnalysisPredictionStrategyHelpers
            .PerformClustering(co, config.MainNumbersCount);
        var main = ClusteringAnalysisPredictionStrategyHelpers
            .SelectNumbersFromClusters(clusters, config.MainNumbersCount).ToImmutableArray();
        var bonus = config.BonusNumbersCount > 0
            ? ClusteringAnalysisPredictionStrategyHelpers.GenerateRandomNumbers(
                1, config.BonusNumbersRange, new List<int>(), config.BonusNumbersCount, random)
            : ImmutableArray<int>.Empty;

        return new PredictionResult(
            config.LotteryId, main, bonus,
            ClusteringAnalysisPredictionStrategyHelpers.CalculateClusteringAnalysisConfidence(clusters, main),
            PredictionAlgorithmKeys.ClusteringAnalysis);
    }
}