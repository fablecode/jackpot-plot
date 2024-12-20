using System.Collections.Immutable;
using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Interfaces;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Repositories;
using JackpotPlot.Domain.ValueObjects;

namespace JackpotPlot.Domain.Services.PredictionStrategies;

public sealed class FrequencyPredictionStrategy : IPredictionStrategy
{
    private readonly ILotteryConfigurationRepository _lotteryConfigurationRepository;
    private readonly ILotteryHistoryRepository _lotteryHistoryRepository;

    public FrequencyPredictionStrategy(ILotteryConfigurationRepository lotteryConfigurationRepository, ILotteryHistoryRepository lotteryHistoryRepository)
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

        // Step 2: Fetch historical draw data
        var historicalDraws = await _lotteryHistoryRepository.GetHistoricalDraws(lotteryId);
        if (!historicalDraws.Any())
            return Result<PredictionResult>.Failure($"No historical draws available for lottery ID: {lotteryId}.");

        // Step 3: Calculate frequency of main and bonus numbers
        var mainNumberFrequency = CalculateFrequency(historicalDraws.SelectMany(d => d.WinningNumbers), lotteryConfiguration.MainNumbersRange);
        var bonusNumberFrequency = lotteryConfiguration.BonusNumbersCount > 0
            ? CalculateFrequency(historicalDraws.SelectMany(d => d.BonusNumbers), lotteryConfiguration.BonusNumbersRange)
            : new List<NumberFrequency>();

        // Step 4: Select the most frequent numbers
        var predictedMainNumbers = mainNumberFrequency
            .OrderByDescending(f => f.Frequency)
            .Take(lotteryConfiguration.MainNumbersCount)
            .Select(f => f.Number)
            .ToList();

        var predictedBonusNumbers = bonusNumberFrequency
            .OrderByDescending(f => f.Frequency)
            .Take(lotteryConfiguration.BonusNumbersCount)
            .Select(f => f.Number)
            .ToImmutableArray();

        // Step 5: Return prediction result
        var predictionResult = new PredictionResult
        (
            lotteryId,
            predictedMainNumbers.ToImmutableArray(),
            predictedBonusNumbers,
            CalculateFrequencyBasedConfidence(historicalDraws, predictedMainNumbers),
            PredictionStrategyType.FrequencyBased
        );

        return Result<PredictionResult>.Success(predictionResult);
    }

    public bool Handles(string strategy)
    {
        return strategy.Equals(PredictionStrategyType.FrequencyBased, StringComparison.OrdinalIgnoreCase);
    }

    #region Private Helpers

    private static List<NumberFrequency> CalculateFrequency(IEnumerable<int> numbers, int range)
    {
        var frequency = new int[range + 1]; // Range is 1-based (e.g., 1-50)
        foreach (var number in numbers)
        {
            frequency[number]++;
        }

        return frequency
            .Select((count, number) => new NumberFrequency(number, count ))
            .Where(f => f.Number > 0) // Ignore 0 index
            .ToList();
    }

    private static double CalculateFrequencyBasedConfidence(ICollection<HistoricalDraw> historicalDraws, List<int> predictedNumbers)
    {
        var matchCount = 0;
        var totalDraws = historicalDraws.Count;

        foreach (var draw in historicalDraws)
        {
            matchCount += draw.WinningNumbers.Intersect(predictedNumbers).Count();
        }

        return (double)matchCount / (totalDraws * predictedNumbers.Count); // Fraction of correct predictions
    }

    #endregion
}