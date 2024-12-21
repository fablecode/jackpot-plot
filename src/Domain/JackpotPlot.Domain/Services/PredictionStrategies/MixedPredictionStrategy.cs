using System.Collections.Immutable;
using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Interfaces;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Repositories;
using JackpotPlot.Domain.ValueObjects;

namespace JackpotPlot.Domain.Services.PredictionStrategies;

public class MixedPredictionStrategy : IPredictionStrategy
{
    private readonly List<IPredictionStrategy> _strategies;
    private readonly Dictionary<string, double> _weights;
    private readonly ILotteryConfigurationRepository _lotteryConfigurationRepository;
    private readonly ILotteryHistoryRepository _lotteryHistoryRepository;

    public MixedPredictionStrategy(List<IPredictionStrategy> strategies, Dictionary<string, double> weights, ILotteryConfigurationRepository lotteryConfigurationRepository, ILotteryHistoryRepository lotteryHistoryRepository)
    {
        _strategies = strategies;
        _weights = weights;
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

        // Step 3: Generate predictions from each strategy
        var strategyResults = new List<PredictionResult>();
        foreach (var strategy in _strategies)
        {
            var result = await strategy.Predict(lotteryId);
            strategyResults.Add(result.Value);
        }

        // Step 4: Combine predictions from all strategies
        var combinedNumbers = CombinePredictions(strategyResults, lotteryConfiguration.MainNumbersCount);
        var bonusNumbers = CombineBonusPredictions(strategyResults, lotteryConfiguration.BonusNumbersCount);

        // Step 5: Calculate combined confidence score
        var combinedConfidence = CalculateCombinedConfidence(strategyResults);

        var predictionResult = new PredictionResult
        (
            lotteryId,
            combinedNumbers.ToImmutableArray(),
            bonusNumbers.ToImmutableArray(),
            combinedConfidence,
            PredictionStrategyType.Mixed
        );

        return Result<PredictionResult>.Success(predictionResult);
    }

    public bool Handles(string strategy)
    {
        return strategy.Equals(PredictionStrategyType.Mixed, StringComparison.OrdinalIgnoreCase);
    }

    #region Private Helpers

    private List<int> CombinePredictions(List<PredictionResult> results, int count)
    {
        var weightedNumbers = new Dictionary<int, double>();

        // Aggregate predictions from all strategies
        foreach (var result in results)
        {
            var weight = _weights.ContainsKey(result.Strategy) ? _weights[result.Strategy] : 1.0;

            foreach (var number in result.PredictedNumbers)
            {
                if (!weightedNumbers.ContainsKey(number))
                {
                    weightedNumbers[number] = 0;
                }

                weightedNumbers[number] += weight;
            }
        }

        // Select numbers with the highest combined weights
        return weightedNumbers
            .OrderByDescending(kv => kv.Value)
            .Take(count)
            .Select(kv => kv.Key)
            .ToList();
    }

    private static List<int> CombineBonusPredictions(List<PredictionResult> results, int count)
    {
        var bonusNumbers = results
            .SelectMany(result => result.BonusNumbers)
            .GroupBy(num => num)
            .OrderByDescending(group => group.Count())
            .Select(group => group.Key)
            .Take(count)
            .ToList();

        return bonusNumbers;
    }

    private double CalculateCombinedConfidence(List<PredictionResult> results)
    {
        double totalConfidence = 0;
        double totalWeight = 0;

        foreach (var result in results)
        {
            var weight = _weights.ContainsKey(result.Strategy) ? _weights[result.Strategy] : 1.0;
            totalConfidence += result.ConfidenceScore * weight;
            totalWeight += weight;
        }

        return totalWeight == 0 ? 0 : totalConfidence / totalWeight;
    }

    #endregion
}