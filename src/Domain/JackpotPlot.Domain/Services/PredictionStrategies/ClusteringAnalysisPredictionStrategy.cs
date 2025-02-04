using System.Collections.Immutable;
using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Interfaces;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Repositories;
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
        var coOccurrenceMatrix = GenerateCoOccurrenceMatrix(historicalDraws, lotteryConfiguration.MainNumbersRange);

        // Step 4: Apply clustering (e.g., K-Means)
        var clusters = PerformClustering(coOccurrenceMatrix, clusterCount: lotteryConfiguration.MainNumbersCount);

        // Step 5: Select numbers from clusters
        var predictedNumbers = SelectNumbersFromClusters(clusters, lotteryConfiguration.MainNumbersCount);

        // Step 6: Generate random bonus numbers (if applicable)
        var random = new Random();
        var bonusNumbers = lotteryConfiguration.BonusNumbersCount > 0
            ? GenerateRandomNumbers(1, lotteryConfiguration.BonusNumbersRange, new List<int>(), lotteryConfiguration.BonusNumbersCount, random)
            : ImmutableArray<int>.Empty;

        var predictionResult = new PredictionResult
        (
            lotteryId,
            predictedNumbers.ToImmutableArray(),
            bonusNumbers,
            CalculateClusteringAnalysisConfidence(clusters, predictedNumbers), // Example confidence score
            PredictionStrategyType.ClusteringAnalysis
        );

        return Result<PredictionResult>.Success(predictionResult);
    }

    public bool Handles(string strategy)
    {
        return !string.IsNullOrWhiteSpace(strategy) && strategy.Equals(PredictionStrategyType.ClusteringAnalysis, StringComparison.OrdinalIgnoreCase);
    }

    #region Private Helpers

    private static int[,] GenerateCoOccurrenceMatrix(ICollection<HistoricalDraw> historicalDraws, int numberRange)
    {
        var matrix = new int[numberRange + 1, numberRange + 1];

        foreach (var draw in historicalDraws)
        {
            var numbers = draw.WinningNumbers;
            for (int i = 0; i < numbers.Count; i++)
            {
                for (int j = i + 1; j < numbers.Count; j++)
                {
                    matrix[numbers[i], numbers[j]]++;
                    matrix[numbers[j], numbers[i]]++; // Symmetric co-occurrence
                }
            }
        }

        return matrix;
    }

    private static List<List<int>> PerformClustering(int[,] coOccurrenceMatrix, int clusterCount)
    {
        // Simplified clustering logic: Use co-occurrence frequencies to group numbers
        var clusters = new List<List<int>>();
        var random = new Random();

        for (int i = 0; i < clusterCount; i++)
        {
            clusters.Add(new List<int>());
        }

        for (int number = 1; number < coOccurrenceMatrix.GetLength(0); number++)
        {
            int assignedCluster = random.Next(0, clusterCount); // Randomly assign to a cluster
            clusters[assignedCluster].Add(number);
        }

        return clusters;
    }

    private static List<int> SelectNumbersFromClusters(List<List<int>> clusters, int numberCount)
    {
        var selectedNumbers = new List<int>();
        var random = new Random();

        foreach (var cluster in clusters)
        {
            if (selectedNumbers.Count >= numberCount)
                break;

            if (cluster.Any())
            {
                selectedNumbers.Add(cluster[random.Next(cluster.Count)]);
            }
        }

        return selectedNumbers.Take(numberCount).ToList();
    }

    private static ImmutableArray<int> GenerateRandomNumbers(int min, int max, List<int> exclude, int count, Random random)
    {
        var availableNumbers = Enumerable.Range(min, max - min + 1).Except(exclude).ToList();
        return availableNumbers.OrderBy(_ => random.Next()).Take(count).ToImmutableArray();
    }

    private double CalculateClusteringAnalysisConfidence(List<List<int>> clusters, List<int> predictedNumbers)
    {
        int correctPredictions = 0;

        foreach (var number in predictedNumbers)
        {
            if (clusters.Any(cluster => cluster.Contains(number)))
            {
                correctPredictions++;
            }
        }

        return (double)correctPredictions / predictedNumbers.Count;
    }


    #endregion
}