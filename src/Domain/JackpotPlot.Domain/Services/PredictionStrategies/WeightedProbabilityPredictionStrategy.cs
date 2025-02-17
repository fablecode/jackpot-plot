using System.Collections.Immutable;
using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Interfaces;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Repositories;
using JackpotPlot.Domain.Services.PredictionStrategies.Attributes;
using JackpotPlot.Domain.ValueObjects;

namespace JackpotPlot.Domain.Services.PredictionStrategies;

[PredictionStrategyDescription(PredictionStrategyType.WeightedProbability, "Assigns weights to each number based on its historical frequency and selects numbers through weighted random sampling, favoring those with higher or lower weights as needed.")]
public class WeightedProbabilityPredictionStrategy : IPredictionStrategy
{
    private readonly ILotteryConfigurationRepository _lotteryConfigurationRepository;
    private readonly ILotteryHistoryRepository _lotteryHistoryRepository;

    public WeightedProbabilityPredictionStrategy(ILotteryConfigurationRepository lotteryConfigurationRepository, ILotteryHistoryRepository lotteryHistoryRepository)
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


        // Step 1: Calculate weights based on historical data
        var weights = CalculateWeights(historicalDraws, lotteryConfiguration.MainNumbersRange);

        // Step 2: Select numbers using weighted random sampling
        var predictedNumbers = WeightedRandomSampling(weights, lotteryConfiguration.MainNumbersCount);

        // Step 3: Generate random bonus numbers (if applicable)
        var random = new Random();
        var bonusNumbers = lotteryConfiguration.BonusNumbersCount > 0
            ? GenerateRandomNumbers(1, lotteryConfiguration.BonusNumbersRange, new List<int>(), lotteryConfiguration.BonusNumbersCount, random)
            : ImmutableArray<int>.Empty;

        var predictionResult = new PredictionResult
        (
            lotteryId,
            predictedNumbers,
            bonusNumbers,
            CalculateWeightedProbabilityConfidence(historicalDraws, weights), // Example confidence score
            PredictionStrategyType.WeightedProbability
        );

        return Result<PredictionResult>.Success(predictionResult);
    }

    public bool Handles(string strategy)
    {
        return strategy.Equals(PredictionStrategyType.WeightedProbability, StringComparison.OrdinalIgnoreCase);
    }

    #region Private Helper

    private static Dictionary<int, double> CalculateWeights(ICollection<HistoricalDraw> historicalDraws, int numberRange)
    {
        var frequency = new int[numberRange + 1];

        foreach (var draw in historicalDraws)
        {
            foreach (var number in draw.WinningNumbers)
            {
                frequency[number]++;
            }
        }

        // Normalize weights
        var totalOccurrences = frequency.Sum();
        var weights = new Dictionary<int, double>();

        for (var i = 1; i <= numberRange; i++)
        {
            weights[i] = (double)frequency[i] / totalOccurrences;
        }

        return weights;
    }

    private static ImmutableArray<int> WeightedRandomSampling(Dictionary<int, double> weights, int count)
    {
        var random = new Random();
        var selectedNumbers = new List<int>();

        while (selectedNumbers.Count < count)
        {
            var roll = random.NextDouble(); // Random value between 0 and 1
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

        return selectedNumbers.ToImmutableArray();
    }

    private static ImmutableArray<int> GenerateRandomNumbers(int min, int max, List<int> exclude, int count, Random random)
    {
        var availableNumbers = Enumerable.Range(min, max - min + 1).Except(exclude).ToList();
        return availableNumbers.OrderBy(_ => random.Next()).Take(count).ToImmutableArray();
    }

    private double CalculateWeightedProbabilityConfidence(ICollection<HistoricalDraw> historicalDraws, Dictionary<int, double> weights)
    {
        int correctPredictions = 0;
        int totalPredictions = historicalDraws.Count * weights.Count;

        foreach (var draw in historicalDraws)
        {
            foreach (var number in draw.WinningNumbers)
            {
                if (weights.TryGetValue(number, out var weight))
                {
                    correctPredictions += (int)(weight * 100);
                }
            }
        }

        return (double)correctPredictions / totalPredictions;
    }

    #endregion
}