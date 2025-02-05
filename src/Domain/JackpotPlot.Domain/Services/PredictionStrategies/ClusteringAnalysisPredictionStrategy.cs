using System.Collections.Immutable;
using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Interfaces;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Repositories;
using JackpotPlot.Domain.Services.PredictionStrategies.Helpers;
using JackpotPlot.Domain.ValueObjects;

namespace JackpotPlot.Domain.Services.PredictionStrategies;

public class ClusteringAnalysisPredictionStrategy : IPredictionStrategy
{
    private readonly ILotteryConfigurationRepository _lotteryConfigurationRepository;
    private readonly ILotteryHistoryRepository _lotteryHistoryRepository;

    public ClusteringAnalysisPredictionStrategy(ILotteryConfigurationRepository lotteryConfigurationRepository, ILotteryHistoryRepository lotteryHistoryRepository)
    {
        _lotteryConfigurationRepository = lotteryConfigurationRepository;
        _lotteryHistoryRepository = lotteryHistoryRepository;
    }

    public async Task<Result<PredictionResult>> Predict(int lotteryId)
    {
        // Step 1: Fetch the lottery configuration
        var lotteryConfiguration = await _lotteryConfigurationRepository.GetActiveConfigurationAsync(lotteryId);
        if (lotteryConfiguration == null)
            return Result<PredictionResult>.Failure($"Lottery configuration not found for ID: {lotteryId}");

        // Step 2: Fetch historical draws
        var historicalDraws = await _lotteryHistoryRepository.GetHistoricalDraws(lotteryId);
        if (!historicalDraws.Any())
            return Result<PredictionResult>.Failure($"No historical draws available for lottery ID: {lotteryId}.");

        // Step 3: Generate co-occurrence matrix
        var coOccurrenceMatrix = ClusteringAnalysisPredictionStrategyHelpers.GenerateCoOccurrenceMatrix(historicalDraws, lotteryConfiguration.MainNumbersRange);

        // Step 4: Apply clustering (e.g., K-Means)
        var clusters = ClusteringAnalysisPredictionStrategyHelpers.PerformClustering(coOccurrenceMatrix, clusterCount: lotteryConfiguration.MainNumbersCount);

        // Step 5: Select numbers from clusters
        var predictedNumbers = ClusteringAnalysisPredictionStrategyHelpers.SelectNumbersFromClusters(clusters, lotteryConfiguration.MainNumbersCount);

        // Step 6: Generate random bonus numbers (if applicable)
        var random = new Random();
        var bonusNumbers = lotteryConfiguration.BonusNumbersCount > 0
            ? ClusteringAnalysisPredictionStrategyHelpers.GenerateRandomNumbers(1, lotteryConfiguration.BonusNumbersRange, new List<int>(), lotteryConfiguration.BonusNumbersCount, random)
            : ImmutableArray<int>.Empty;

        var predictionResult = new PredictionResult
        (
            lotteryId,
            predictedNumbers.ToImmutableArray(),
            bonusNumbers,
            ClusteringAnalysisPredictionStrategyHelpers.CalculateClusteringAnalysisConfidence(clusters, predictedNumbers), // Example confidence score
            PredictionStrategyType.ClusteringAnalysis
        );

        return Result<PredictionResult>.Success(predictionResult);
    }

    public bool Handles(string strategy)
    {
        return !string.IsNullOrWhiteSpace(strategy) && strategy.Equals(PredictionStrategyType.ClusteringAnalysis, StringComparison.OrdinalIgnoreCase);
    }
}