using System.Collections.Immutable;
using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Interfaces;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Repositories;
using JackpotPlot.Domain.ValueObjects;

namespace JackpotPlot.Domain.Services.PredictionStrategies;

public class WeightDistributionPredictionStrategy : IPredictionStrategy
{
    private readonly ILotteryConfigurationRepository _lotteryConfigurationRepository;
    private readonly ILotteryHistoryRepository _lotteryHistoryRepository;

    public WeightDistributionPredictionStrategy(ILotteryConfigurationRepository lotteryConfigurationRepository, ILotteryHistoryRepository lotteryHistoryRepository)
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

        // Step 3: Calculate weights for all numbers based on historical data
        var weights = CalculateWeights(historicalDraws, lotteryConfiguration.MainNumbersRange);

        // Step 4: Generate predictions using weighted sampling
        var predictedNumbers = GenerateWeightedRandomNumbers(weights, lotteryConfiguration.MainNumbersCount);

        // Step 5: Generate random bonus numbers (if applicable)
        var random = new Random();
        var bonusNumbers = lotteryConfiguration.BonusNumbersCount > 0
            ? GenerateRandomNumbers(1, lotteryConfiguration.BonusNumbersRange, lotteryConfiguration.BonusNumbersCount, random)
            : new List<int>();

        var predictionResult = new PredictionResult
        (
            lotteryId,
            predictedNumbers.ToImmutableArray(),
            bonusNumbers.ToImmutableArray(),
            CalculateWeightDistributionConfidence(historicalDraws, predictedNumbers),
            PredictionStrategyType.WeightDistribution
        );

        return Result<PredictionResult>.Success(predictionResult);
    }

    public bool Handles(string strategy)
    {
        return strategy.Equals(PredictionStrategyType.WeightDistribution, StringComparison.OrdinalIgnoreCase);
    }

    #region Private Helpers

    private static Dictionary<int, double> CalculateWeights(ICollection<HistoricalDraw> historicalDraws, int numberRange)
    {
        // Initialize weights with zero
        var weights = new Dictionary<int, double>();
        for (int i = 1; i <= numberRange; i++)
        {
            weights[i] = 0;
        }

        // Count occurrences of each number in historical data
        foreach (var draw in historicalDraws)
        {
            foreach (var number in draw.WinningNumbers)
            {
                weights[number]++;
            }
        }

        // Normalize weights to sum to 1
        double totalWeight = weights.Values.Sum();
        foreach (var key in weights.Keys.ToList())
        {
            weights[key] /= totalWeight;
        }

        return weights;
    }

    private static List<int> GenerateWeightedRandomNumbers(Dictionary<int, double> weights, int count)
    {
        var random = new Random();
        var selectedNumbers = new List<int>();

        while (selectedNumbers.Count < count)
        {
            double roll = random.NextDouble(); // Random value between 0 and 1
            double cumulativeWeight = 0;

            foreach (var (number, weight) in weights)
            {
                cumulativeWeight += weight;
                if (roll <= cumulativeWeight && !selectedNumbers.Contains(number))
                {
                    selectedNumbers.Add(number);
                    break;
                }
            }
        }

        return selectedNumbers.OrderBy(_ => random.Next()).ToList(); // Shuffle results
    }

    private static double CalculateWeightDistributionConfidence(ICollection<HistoricalDraw> historicalDraws, List<int> predictedNumbers)
    {
        int matchCount = 0;
        int totalOccurrences = 0;

        foreach (var draw in historicalDraws)
        {
            foreach (var number in predictedNumbers)
            {
                if (draw.WinningNumbers.Contains(number))
                {
                    matchCount++;
                }
                totalOccurrences++;
            }
        }

        return (double)matchCount / totalOccurrences;
    }

    private static List<int> GenerateRandomNumbers(int min, int max, int count, Random random)
    {
        return Enumerable.Range(min, max - min + 1)
                         .OrderBy(_ => random.Next())
                         .Take(count)
                         .ToList();
    }

    #endregion
}