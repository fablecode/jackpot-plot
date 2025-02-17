using System.Collections.Immutable;
using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Interfaces;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Repositories;
using JackpotPlot.Domain.Services.PredictionStrategies.Attributes;
using JackpotPlot.Domain.ValueObjects;

namespace JackpotPlot.Domain.Services.PredictionStrategies;

[PredictionStrategyDescription(PredictionStrategyType.LastAppearance, "Prioritizes numbers that have not appeared for a long time, considering them “overdue” and likely to be drawn in the next draw.")]
public class LastAppearancePredictionStrategy : IPredictionStrategy
{
    private readonly ILotteryConfigurationRepository _lotteryConfigurationRepository;
    private readonly ILotteryHistoryRepository _lotteryHistoryRepository;

    public LastAppearancePredictionStrategy(ILotteryConfigurationRepository lotteryConfigurationRepository, ILotteryHistoryRepository lotteryHistoryRepository)
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

        // Step 3: Track the last appearance of each number
        var lastAppearances = TrackLastAppearances(historicalDraws.ToList(), lotteryConfiguration.MainNumbersRange);

        // Step 4: Select the most overdue numbers
        var overdueNumbers = SelectMostOverdueNumbers(lastAppearances, lotteryConfiguration.MainNumbersCount);

        // Step 5: Generate random bonus numbers (if applicable)
        var random = new Random();
        var bonusNumbers = lotteryConfiguration.BonusNumbersCount > 0
            ? GenerateRandomNumbers(1, lotteryConfiguration.BonusNumbersRange, new List<int>(), lotteryConfiguration.BonusNumbersCount, random)
            : ImmutableArray<int>.Empty;

        var predictionResult = new PredictionResult
        (
            lotteryId,
            overdueNumbers.ToImmutableArray(),
            bonusNumbers,
            CalculateLastAppearanceConfidence(historicalDraws, overdueNumbers), // Example confidence score
            PredictionStrategyType.LastAppearance
        );

        return Result<PredictionResult>.Success(predictionResult);
    }

    public bool Handles(string strategy)
    {
        return strategy.Equals(PredictionStrategyType.LastAppearance, StringComparison.OrdinalIgnoreCase);
    }

    #region Private Helpers

    private static Dictionary<int, int> TrackLastAppearances(IList<HistoricalDraw> historicalDraws, int numberRange)
    {
        var lastAppearances = new Dictionary<int, int>();

        // Initialize all numbers as "not yet drawn"
        for (int i = 1; i <= numberRange; i++)
        {
            lastAppearances[i] = int.MaxValue; // Use a large value to signify no appearance
        }

        // Traverse historical draws in reverse order
        for (int drawIndex = historicalDraws.Count - 1; drawIndex >= 0; drawIndex--)
        {
            var draw = historicalDraws[drawIndex];
            foreach (var number in draw.WinningNumbers)
            {
                if (lastAppearances[number] == int.MaxValue) // Update only if it hasn't been recorded
                {
                    lastAppearances[number] = historicalDraws.Count - drawIndex; // "Days since last appearance"
                }
            }
        }

        return lastAppearances;
    }

    private static List<int> SelectMostOverdueNumbers(Dictionary<int, int> lastAppearances, int count)
    {
        return lastAppearances
            .OrderByDescending(kv => kv.Value) // Sort by "days since last appearance" in descending order
            .ThenBy(kv => kv.Key)             // Resolve ties by number order
            .Take(count)
            .Select(kv => kv.Key)
            .ToList();
    }

    private static ImmutableArray<int> GenerateRandomNumbers(int min, int max, List<int> exclude, int count, Random random)
    {
        var availableNumbers = Enumerable.Range(min, max - min + 1).Except(exclude).ToList();
        return availableNumbers.OrderBy(_ => random.Next()).Take(count).ToImmutableArray();
    }

    private double CalculateLastAppearanceConfidence(ICollection<HistoricalDraw> historicalDraws, List<int> predictedNumbers)
    {
        int matchCount = 0;

        foreach (var draw in historicalDraws)
        {
            matchCount += draw.WinningNumbers.Intersect(predictedNumbers).Count();
        }

        return (double)matchCount / (historicalDraws.Count * predictedNumbers.Count);
    }

    #endregion
}