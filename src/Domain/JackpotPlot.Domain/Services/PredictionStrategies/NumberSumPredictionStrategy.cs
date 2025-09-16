using System.Collections.Immutable;
using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Interfaces;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Repositories;
using JackpotPlot.Domain.Services.PredictionStrategies.Attributes;
using JackpotPlot.Domain.ValueObjects;

namespace JackpotPlot.Domain.Services.PredictionStrategies;

[PredictionStrategyDescription(PredictionStrategyType.NumberSum, "Focuses on the overall sum of numbers in past draws. It calculates an average (or target) sum from historical data and selects numbers that, when combined, approximate this total.")]
public class NumberSumPredictionStrategy : IPredictionStrategy
{
    private readonly ILotteryConfigurationRepository _lotteryConfigurationRepository;
    private readonly ILotteryHistoryRepository _lotteryHistoryRepository;

    public NumberSumPredictionStrategy(ILotteryConfigurationRepository lotteryConfigurationRepository, ILotteryHistoryRepository lotteryHistoryRepository)
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


        // Step 1: Calculate the target sum based on historical averages
        var targetSum = CalculateTargetSum(historicalDraws);

        // Step 2: Generate numbers that sum close to the target
        var predictedNumbers = GenerateNumbersWithTargetSum(lotteryConfiguration.MainNumbersRange, lotteryConfiguration.MainNumbersCount, targetSum);

        // Step 3: Generate random bonus numbers (if applicable)
        var random = new Random();
        var bonusNumbers = lotteryConfiguration.BonusNumbersCount > 0
            ? GenerateRandomNumbers(1, lotteryConfiguration.BonusNumbersRange, lotteryConfiguration.BonusNumbersCount, random)
            : [];

        var predictionResult = new PredictionResult
        (
            lotteryId,
            predictedNumbers.ToImmutableArray(),
            bonusNumbers.ToImmutableArray(),
            CalculateNumberSumConfidence(historicalDraws, predictedNumbers),
            PredictionStrategyType.NumberSum
        );

        return Result<PredictionResult>.Success(predictionResult);
    }

    public bool Handles(string strategy)
    {
        return !string.IsNullOrWhiteSpace(strategy) && strategy.Equals(PredictionStrategyType.NumberSum, StringComparison.OrdinalIgnoreCase);
    }

    private static double CalculateTargetSum(ICollection<HistoricalDraw> historicalDraws)
    {
        // Average the sums of numbers in all historical draws
        return historicalDraws.Select(draw => draw.WinningNumbers.Sum()).Average();
    }

    private static List<int> GenerateNumbersWithTargetSum(int maxRange, int count, double targetSum)
    {
        var random = new Random();
        var numbers = new List<int>();

        while (numbers.Count < count)
        {
            var remainingCount = count - numbers.Count;
            var remainingSum = targetSum - numbers.Sum();

            // Generate a random number within the valid range
            var nextNumber = Math.Min(maxRange, Math.Max(1, (int)(remainingSum / remainingCount + random.Next(-3, 3))));

            if (!numbers.Contains(nextNumber))
            {
                numbers.Add(nextNumber);
            }
        }

        return numbers.OrderBy(_ => random.Next()).ToList();
    }

    private static double CalculateNumberSumConfidence(ICollection<HistoricalDraw> historicalDraws, List<int> predictedNumbers)
    {
        // Calculate the sum of the predicted numbers
        double predictedSum = predictedNumbers.Sum();

        // Compare the predicted sum to historical sums
        var deviation = historicalDraws
            .Select(draw => Math.Abs(draw.WinningNumbers.Sum() - predictedSum))
            .Average();

        return 1.0 / (1.0 + deviation); // Inverse of the average deviation
    }

    private static List<int> GenerateRandomNumbers(int min, int max, int count, Random random)
    {
        return Enumerable.Range(min, max - min + 1)
            .OrderBy(_ => random.Next())
            .Take(count)
            .ToList();
    }
}