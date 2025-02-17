using System.Collections.Immutable;
using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Interfaces;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Repositories;
using JackpotPlot.Domain.Services.PredictionStrategies.Attributes;
using JackpotPlot.Domain.ValueObjects;

namespace JackpotPlot.Domain.Services.PredictionStrategies;

[PredictionStrategyDescription(PredictionStrategyType.RepeatingNumbers, "Focuses on numbers that have appeared frequently in recent draws, under the assumption that such numbers might continue to repeat.")]
public class RepeatingNumbersPredictionStrategy : IPredictionStrategy
{
    private readonly ILotteryConfigurationRepository _lotteryConfigurationRepository;
    private readonly ILotteryHistoryRepository _lotteryHistoryRepository;

    private readonly int _recentDrawsToConsider = 10;

    public RepeatingNumbersPredictionStrategy(ILotteryConfigurationRepository lotteryConfigurationRepository, ILotteryHistoryRepository lotteryHistoryRepository)
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

        // Step 3: Collect numbers from recent draws
        var recentNumbers = GetRecentNumbers(historicalDraws);

        // Step 4: Identify repeating numbers
        var repeatingNumbers = IdentifyRepeatingNumbers(recentNumbers);

        // Step 5: Generate predictions from repeating numbers
        var predictedNumbers = GeneratePredictionsFromRepeatingNumbers(repeatingNumbers, lotteryConfiguration.MainNumbersCount);

        // Step 6: Generate random bonus numbers (if applicable)
        var random = new Random();
        var bonusNumbers = lotteryConfiguration.BonusNumbersCount > 0
            ? GenerateRandomNumbers(1, lotteryConfiguration.BonusNumbersRange, lotteryConfiguration.BonusNumbersCount, random)
            : new List<int>();

        var predictionResult = new PredictionResult
        (
            lotteryId,
            predictedNumbers.ToImmutableArray(),
            bonusNumbers.ToImmutableArray(),
            CalculateRepeatingNumbersConfidence(historicalDraws, predictedNumbers),
            PredictionStrategyType.RepeatingNumbers
        );

        return Result<PredictionResult>.Success(predictionResult);
    }

    public bool Handles(string strategy)
    {
        return strategy.Equals(PredictionStrategyType.RepeatingNumbers, StringComparison.OrdinalIgnoreCase);
    }

    #region Private Helpers

    public List<int> GetRecentNumbers(ICollection<HistoricalDraw> historicalDraws)
    {
        return historicalDraws
            .Take(_recentDrawsToConsider)
            .SelectMany(draw => draw.WinningNumbers)
            .ToList();
    }

    private static Dictionary<int, int> IdentifyRepeatingNumbers(List<int> numbers)
    {
        var numberFrequency = new Dictionary<int, int>();

        foreach (var number in numbers)
        {
            if (!numberFrequency.ContainsKey(number))
            {
                numberFrequency[number] = 0;
            }

            numberFrequency[number]++;
        }

        return numberFrequency
            .Where(kv => kv.Value > 1) // Keep only repeating numbers
            .OrderByDescending(kv => kv.Value) // Sort by frequency
            .ToDictionary(kv => kv.Key, kv => kv.Value);
    }

    private static List<int> GeneratePredictionsFromRepeatingNumbers(Dictionary<int, int> repeatingNumbers, int count)
    {
        return repeatingNumbers
            .Keys
            .Take(count)
            .ToList();
    }

    private static double CalculateRepeatingNumbersConfidence(ICollection<HistoricalDraw> historicalDraws, List<int> predictedNumbers)
    {
        int matchCount = 0;

        foreach (var draw in historicalDraws)
        {
            matchCount += draw.WinningNumbers.Intersect(predictedNumbers).Count();
        }

        return (double)matchCount / (historicalDraws.Count * predictedNumbers.Count);
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