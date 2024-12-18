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
            .ToImmutableArray();

        var predictedBonusNumbers = bonusNumberFrequency
            .OrderByDescending(f => f.Frequency)
            .Take(lotteryConfiguration.BonusNumbersCount)
            .Select(f => f.Number)
            .ToImmutableArray();

        // Step 5: Return prediction result
        var predictionResult = new PredictionResult
        (
            lotteryId,
            predictedMainNumbers,
            predictedBonusNumbers,
            CalculateConfidenceScore(predictedMainNumbers, predictedBonusNumbers),
            PredictionStrategyType.FrequencyBased
        );

        return Result<PredictionResult>.Success(predictionResult);
    }

    public bool Handles(string strategy)
    {
        return strategy.Equals(PredictionStrategyType.FrequencyBased, StringComparison.OrdinalIgnoreCase);
    }

    #region Private Helpers

    private List<NumberFrequency> CalculateFrequency(IEnumerable<int> numbers, int range)
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

    private double CalculateConfidenceScore(ImmutableArray<int> mainNumbers, ImmutableArray<int> bonusNumbers)
    {
        // Example: Calculate confidence score based on average frequency
        var totalNumbers = mainNumbers.Length + bonusNumbers.Length;
        return totalNumbers > 0 ? 0.8 : 0.0; // Example static confidence score
    }

    #endregion
}