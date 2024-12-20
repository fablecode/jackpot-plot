using System.Collections.Immutable;
using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Interfaces;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Repositories;
using JackpotPlot.Domain.ValueObjects;

namespace JackpotPlot.Domain.Services.PredictionStrategies;

public class TimeDecayPredictionStrategy : IPredictionStrategy
{
    private readonly ILotteryConfigurationRepository _lotteryConfigurationRepository;
    private readonly ILotteryHistoryRepository _lotteryHistoryRepository;

    public TimeDecayPredictionStrategy(ILotteryConfigurationRepository lotteryConfigurationRepository, ILotteryHistoryRepository lotteryHistoryRepository)
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

        // Step 3: Calculate time-decayed weights for each draw
        var drawWeights = AssignTimeDecayWeights(historicalDraws);

        // Step 4: Aggregate weighted frequencies for each number
        var weightedFrequencies = CalculateWeightedFrequencies(historicalDraws, drawWeights, lotteryConfiguration.MainNumbersRange);

        // Step 5: Generate predictions using weighted sampling
        var predictedNumbers = GenerateWeightedRandomNumbers(weightedFrequencies, lotteryConfiguration.MainNumbersCount);

        // Step 6: Generate random bonus numbers (if applicable)
        var random = new Random();
        var bonusNumbers = lotteryConfiguration.BonusNumbersCount > 0
            ? GenerateRandomNumbers(1, lotteryConfiguration.BonusNumbersRange, lotteryConfiguration.BonusNumbersCount, random)
            : new List<int>();

        var predictionResult = new PredictionResult
        (
            lotteryId,
            predictedNumbers.ToImmutableArray(),
            bonusNumbers.ToImmutableArray(),
            CalculateTimeDecayConfidence(historicalDraws, predictedNumbers),
            PredictionStrategyType.TimeDecay
        );

        return Result<PredictionResult>.Success(predictionResult);
    }

    public bool Handles(string strategy)
    {
        return strategy.Equals(PredictionStrategyType.TimeDecay, StringComparison.OrdinalIgnoreCase);
    }

    #region Private Helpers

    private static Dictionary<int, double> AssignTimeDecayWeights(ICollection<HistoricalDraw> historicalDraws)
    {
        var weights = new Dictionary<int, double>();
        double decayFactor = 0.9; // Exponential decay factor
        int drawIndex = 0;

        foreach (var draw in historicalDraws.OrderByDescending(d => d.DrawDate))
        {
            weights[draw.DrawId] = Math.Pow(decayFactor, drawIndex++);
        }

        return weights;
    }

    private static Dictionary<int, double> CalculateWeightedFrequencies(ICollection<HistoricalDraw> historicalDraws, Dictionary<int, double> drawWeights, int numberRange)
    {
        var frequencies = new Dictionary<int, double>();

        for (int i = 1; i <= numberRange; i++)
        {
            frequencies[i] = 0;
        }

        foreach (var draw in historicalDraws)
        {
            foreach (var number in draw.WinningNumbers)
            {
                frequencies[number] += drawWeights[draw.DrawId];
            }
        }

        return frequencies;
    }

    private static List<int> GenerateWeightedRandomNumbers(Dictionary<int, double> weightedFrequencies, int count)
    {
        var random = new Random();
        var selectedNumbers = new List<int>();

        while (selectedNumbers.Count < count)
        {
            double roll = random.NextDouble(); // Random value between 0 and 1
            double cumulativeWeight = 0;

            foreach (var (number, weight) in weightedFrequencies)
            {
                cumulativeWeight += weight;
                if (roll <= cumulativeWeight && !selectedNumbers.Contains(number))
                {
                    selectedNumbers.Add(number);
                    break;
                }
            }
        }

        return selectedNumbers.OrderBy(_ => random.Next()).ToList();
    }

    private static double CalculateTimeDecayConfidence(ICollection<HistoricalDraw> historicalDraws, List<int> predictedNumbers)
    {
        int matchCount = 0;

        foreach (var draw in historicalDraws.Take(10)) // Focus on the last 10 draws
        {
            matchCount += draw.WinningNumbers.Intersect(predictedNumbers).Count();
        }

        return (double)matchCount / (10 * predictedNumbers.Count);
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