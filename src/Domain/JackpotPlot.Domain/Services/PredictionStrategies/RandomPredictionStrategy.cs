﻿using System.Collections.Immutable;
using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Interfaces;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Repositories;
using JackpotPlot.Domain.Services.PredictionStrategies.Attributes;
using JackpotPlot.Domain.ValueObjects;

namespace JackpotPlot.Domain.Services.PredictionStrategies;

[PredictionStrategyDescription(PredictionStrategyType.Random, "Generate numbers randomly.")]
public sealed class RandomPredictionStrategy : IPredictionStrategy
{
    private readonly ILotteryConfigurationRepository _configurationRepository;

    public RandomPredictionStrategy(ILotteryConfigurationRepository configurationRepository)
    {
        _configurationRepository = configurationRepository;
    }

    public async Task<Result<PredictionResult>> Predict(int lotteryId)
    {
        var lotteryConfiguration = await _configurationRepository.GetActiveConfigurationAsync(lotteryId);
        if (lotteryConfiguration == null)
            return Result<PredictionResult>.Failure($"Lottery configuration not found for ID: {lotteryId}");

        var random = new Random();

        // Generate main numbers
        var mainNumbers = Enumerable.Range(1, lotteryConfiguration.MainNumbersRange)
            .OrderBy(_ => random.Next())
            .Take(lotteryConfiguration.MainNumbersCount)
            .ToImmutableArray();

        // Generate bonus numbers (if applicable)
        var bonusNumbers = lotteryConfiguration.BonusNumbersCount > 0
            ? Enumerable.Range(1, lotteryConfiguration.BonusNumbersRange)
                .OrderBy(_ => random.Next())
                .Take(lotteryConfiguration.BonusNumbersCount)
                .ToImmutableArray()
            : [];

        var predictionResult = new PredictionResult
        (
            lotteryConfiguration.LotteryId,
            mainNumbers,
            bonusNumbers,
            CalculateRandomConfidence(lotteryConfiguration.MainNumbersRange, lotteryConfiguration.MainNumbersCount), // Random predictions are inherently less confident
            PredictionStrategyType.Random
        );

        return Result<PredictionResult>.Success(predictionResult);
    }

    public bool Handles(string strategy)
    {
        return strategy.Equals(PredictionStrategyType.Random, StringComparison.OrdinalIgnoreCase);
    }

    #region Private Helpers

    private static double CalculateRandomConfidence(int mainNumbersRange, int mainNumbersCount)
    {
        return 1.0 / (mainNumbersRange - mainNumbersCount + 1); // Probability of a single correct prediction
    }

    #endregion
}