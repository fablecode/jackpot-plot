using System.Collections.Immutable;
using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Interfaces;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Repositories;
using JackpotPlot.Domain.ValueObjects;

namespace JackpotPlot.Domain.Services.PredictionStrategies;

public class SkewnessAnalysisPredictionStrategy : IPredictionStrategy
{
    private readonly ILotteryConfigurationRepository _lotteryConfigurationRepository;
    private readonly ILotteryHistoryRepository _lotteryHistoryRepository;

    public SkewnessAnalysisPredictionStrategy(ILotteryConfigurationRepository lotteryConfigurationRepository, ILotteryHistoryRepository lotteryHistoryRepository)
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

        // Step 3: Analyze skewness in historical data
        var skewness = CalculateSkewness(historicalDraws);

        // Step 4: Generate numbers based on skewness
        var predictedNumbers = GenerateNumbersBasedOnSkewness(lotteryConfiguration.MainNumbersRange, lotteryConfiguration.MainNumbersCount, skewness);

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
            CalculateSkewnessConfidence(historicalDraws, predictedNumbers),
            PredictionStrategyType.SkewnessAnalysis
        );

        return Result<PredictionResult>.Success(predictionResult);
    }

    public bool Handles(string strategy)
    {
        return strategy.Equals(PredictionStrategyType.SkewnessAnalysis, StringComparison.OrdinalIgnoreCase);
    }

    #region Private Helpers

    private static double CalculateSkewness(ICollection<HistoricalDraw> historicalDraws)
    {
        var allNumbers = historicalDraws.SelectMany(draw => draw.WinningNumbers).ToList();
        var mean = allNumbers.Average();
        var stdDev = Math.Sqrt(allNumbers.Sum(num => Math.Pow(num - mean, 2)) / allNumbers.Count);

        // Skewness formula
        var skewness = allNumbers.Sum(num => Math.Pow(num - mean, 3)) / allNumbers.Count;
        skewness /= Math.Pow(stdDev, 3);

        return skewness;
    }

    private static List<int> GenerateNumbersBasedOnSkewness(int maxRange, int count, double skewness)
    {
        var random = new Random();
        var numbers = new List<int>();

        while (numbers.Count < count)
        {
            int candidate = random.Next(1, maxRange + 1);

            // Adjust probability based on skewness
            if (skewness < 0 && candidate > maxRange / 2) // Favor higher numbers for negative skew
            {
                numbers.Add(candidate);
            }
            else if (skewness > 0 && candidate <= maxRange / 2) // Favor lower numbers for positive skew
            {
                numbers.Add(candidate);
            }
            else if (Math.Abs(skewness) < 0.1) // Neutral skew: allow all numbers
            {
                numbers.Add(candidate);
            }

            // Ensure uniqueness
            numbers = numbers.Distinct().ToList();
        }

        return numbers.OrderBy(_ => random.Next()).ToList();
    }

    private double CalculateSkewnessConfidence(ICollection<HistoricalDraw> historicalDraws, List<int> predictedNumbers)
    {
        var actualSkewness = CalculateSkewness(historicalDraws);
        var predictedMean = predictedNumbers.Average();
        var actualMean = historicalDraws.SelectMany(draw => draw.WinningNumbers).Average();

        return 1.0 / (1.0 + Math.Abs(predictedMean - actualMean) + Math.Abs(actualSkewness));
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