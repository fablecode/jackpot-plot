using System.Collections.Immutable;
using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Interfaces;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Repositories;
using JackpotPlot.Domain.Services.PredictionStrategies.Attributes;
using JackpotPlot.Domain.ValueObjects;

namespace JackpotPlot.Domain.Services.PredictionStrategies;

[PredictionStrategyDescription(PredictionStrategyType.StandardDeviation, "Uses the spread (standard deviation) of historical draws to generate predictions that mimic the overall variability in the numbers.")]
public class StandardDeviationPredictionStrategy : IPredictionStrategy
{
    private readonly ILotteryConfigurationRepository _lotteryConfigurationRepository;
    private readonly ILotteryHistoryRepository _lotteryHistoryRepository;

    public StandardDeviationPredictionStrategy(ILotteryConfigurationRepository lotteryConfigurationRepository, ILotteryHistoryRepository lotteryHistoryRepository)
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

        // Step 3: Calculate historical standard deviation
        var targetStdDev = CalculateHistoricalStandardDeviation(historicalDraws);

        // Step 4: Generate numbers that match the target standard deviation
        var predictedNumbers = GenerateNumbersWithTargetStdDev(lotteryConfiguration.MainNumbersRange, lotteryConfiguration.MainNumbersCount, targetStdDev);

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
            CalculateStandardDeviationConfidence(historicalDraws, predictedNumbers),
            PredictionStrategyType.StandardDeviation
        );

        return Result<PredictionResult>.Success(predictionResult);
    }

    public bool Handles(string strategy)
    {
        return strategy.Equals(PredictionStrategyType.StandardDeviation, StringComparison.OrdinalIgnoreCase);
    }

    #region Private Helpers

    private static double CalculateHistoricalStandardDeviation(ICollection<HistoricalDraw> historicalDraws)
    {
        var allNumbers = historicalDraws.SelectMany(draw => draw.WinningNumbers).ToList();
        double mean = allNumbers.Average();

        return Math.Sqrt(allNumbers.Sum(num => Math.Pow(num - mean, 2)) / allNumbers.Count);
    }

    private static List<int> GenerateNumbersWithTargetStdDev(int maxRange, int count, double targetStdDev)
    {
        var random = new Random();
        var numbers = new List<int>();

        while (numbers.Count < count)
        {
            int candidate = random.Next(1, maxRange + 1);

            // Ensure numbers produce a spread similar to the target standard deviation
            double currentStdDev = CalculateStandardDeviation(numbers.Concat(new[] { candidate }).ToList());
            if (Math.Abs(currentStdDev - targetStdDev) < 0.5 || numbers.Count == 0)
            {
                numbers.Add(candidate);
            }

            // Ensure uniqueness
            numbers = numbers.Distinct().ToList();
        }

        return numbers.OrderBy(_ => random.Next()).ToList();
    }

    private static double CalculateStandardDeviation(List<int> numbers)
    {
        var mean = numbers.Average();
        return Math.Sqrt(numbers.Sum(num => Math.Pow(num - mean, 2)) / numbers.Count);
    }

    private static double CalculateStandardDeviationConfidence(ICollection<HistoricalDraw> historicalDraws, List<int> predictedNumbers)
    {
        var historicalStdDev = CalculateHistoricalStandardDeviation(historicalDraws);
        var predictedStdDev = CalculateStandardDeviation(predictedNumbers);

        return 1.0 / (1.0 + Math.Abs(historicalStdDev - predictedStdDev));
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