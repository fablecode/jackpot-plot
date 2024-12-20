using System.Collections.Immutable;
using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Interfaces;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Repositories;
using JackpotPlot.Domain.ValueObjects;

namespace JackpotPlot.Domain.Services.PredictionStrategies;

public class StatisticalAveragingPredictionStrategy : IPredictionStrategy
{
    private readonly ILotteryConfigurationRepository _lotteryConfigurationRepository;
    private readonly ILotteryHistoryRepository _lotteryHistoryRepository;

    public StatisticalAveragingPredictionStrategy(ILotteryConfigurationRepository lotteryConfigurationRepository, ILotteryHistoryRepository lotteryHistoryRepository)
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


        // Step 3: Compute averages for main numbers
        var mainNumberAverages = CalculateAverages(historicalDraws, lotteryConfiguration.MainNumbersCount, lotteryConfiguration.MainNumbersRange);

        // Step 4: Compute averages for bonus numbers (if applicable)
        var bonusNumberAverages = lotteryConfiguration.BonusNumbersCount > 0
            ? CalculateAverages(historicalDraws, lotteryConfiguration.BonusNumbersCount, lotteryConfiguration.BonusNumbersRange, true)
            : new List<int>();

        var predictionResult = new PredictionResult
        (
            lotteryId,
            mainNumberAverages.ToImmutableArray(),
            bonusNumberAverages.ToImmutableArray(),
            CalculateStatisticalAveragingConfidence(historicalDraws, mainNumberAverages), // Example confidence score
            PredictionStrategyType.StatisticalAveraging
        );

        return Result<PredictionResult>.Success(predictionResult);
    }

    public bool Handles(string strategy)
    {
        return strategy.Equals(PredictionStrategyType.StatisticalAveraging, StringComparison.OrdinalIgnoreCase);
    }

    #region Private helpers

    private static List<int> CalculateAverages(
        ICollection<HistoricalDraw> historicalDraws,
        int numbersCount,
        int maxRange,
        bool isBonus = false)
    {
        var averages = new List<int>();

        // Step 1: Extract numbers by position
        for (int position = 0; position < numbersCount; position++)
        {
            var numbersAtPosition = historicalDraws
                .Select(draw => isBonus ? draw.BonusNumbers.ElementAtOrDefault(position) : draw.WinningNumbers.ElementAtOrDefault(position))
                .Where(n => n > 0) // Ignore missing numbers
                .ToList();

            // Step 2: Compute the mean and round it
            if (numbersAtPosition.Any())
            {
                var average = Math.Round(numbersAtPosition.Average());
                averages.Add((int)Math.Clamp(average, 1, maxRange)); // Ensure within range
            }
        }

        return averages;
    }

    private double CalculateStatisticalAveragingConfidence(ICollection<HistoricalDraw> historicalDraws, List<int> predictedNumbers)
    {
        var actualAverages = historicalDraws.Select(draw => draw.WinningNumbers.Average()).ToList();
        double predictedAverage = predictedNumbers.Average();

        double deviation = actualAverages.Select(avg => Math.Abs(avg - predictedAverage)).Average();

        return 1.0 / (1.0 + deviation); // Inverse of average deviation
    }

    #endregion
}