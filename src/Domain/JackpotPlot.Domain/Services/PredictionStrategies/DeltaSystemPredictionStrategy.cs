﻿using System.Collections.Immutable;
using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Interfaces;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Repositories;
using JackpotPlot.Domain.ValueObjects;

namespace JackpotPlot.Domain.Services.PredictionStrategies;

public class DeltaSystemPredictionStrategy : IPredictionStrategy
{
    private readonly ILotteryConfigurationRepository _lotteryConfigurationRepository;
    private readonly ILotteryHistoryRepository _lotteryHistoryRepository;

    public DeltaSystemPredictionStrategy(ILotteryConfigurationRepository lotteryConfigurationRepository, ILotteryHistoryRepository lotteryHistoryRepository)
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

        // Step 3: Calculate deltas from historical draws
        var allDeltas = CalculateDeltas(historicalDraws);

        // Step 4: Find the most frequent deltas
        var frequentDeltas = GetFrequentDeltas(allDeltas, lotteryConfiguration.MainNumbersCount);

        // Step 5: Generate predictions using deltas
        var predictedNumbers = GenerateNumbersFromDeltas(frequentDeltas, lotteryConfiguration.MainNumbersRange);

        var predictionResult = new PredictionResult
        (
            lotteryId,
            predictedNumbers, 
            ImmutableArray<int>.Empty, // Adjust for bonus numbers if needed
            0.8, // Example confidence score
            PredictionStrategyType.DeltaSystem
        );

        return Result<PredictionResult>.Success(predictionResult);
    }

    public bool Handles(string strategy)
    {
        return strategy.Equals(PredictionStrategyType.DeltaSystem, StringComparison.OrdinalIgnoreCase);
    }

    #region Private Helpers

    private static List<int> CalculateDeltas(ICollection<HistoricalDraw> historicalDraws)
    {
        var deltas = new List<int>();

        foreach (var draw in historicalDraws)
        {
            var numbers = draw.WinningNumbers.OrderBy(n => n).ToList();

            for (int i = 1; i < numbers.Count; i++)
            {
                deltas.Add(numbers[i] - numbers[i - 1]);
            }
        }

        return deltas;
    }

    private static List<int> GetFrequentDeltas(List<int> deltas, int count)
    {
        return deltas
            .GroupBy(d => d)
            .OrderByDescending(g => g.Count())
            .Take(count)
            .Select(g => g.Key)
            .ToList();
    }

    private static ImmutableArray<int> GenerateNumbersFromDeltas(List<int> deltas, int maxRange)
    {
        var random = new Random();
        var firstNumber = random.Next(1, maxRange / 2); // Start with a random number in the lower half of the range
        var numbers = new List<int> { firstNumber };

        foreach (var delta in deltas)
        {
            var nextNumber = numbers.Last() + delta;

            if (nextNumber > maxRange)
                break; // Stop if the number exceeds the range

            numbers.Add(nextNumber);
        }

        return numbers.ToImmutableArray();
    }

    #endregion
}