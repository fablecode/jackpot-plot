using System.Collections.Immutable;
using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Interfaces;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Repositories;
using JackpotPlot.Domain.Services.PredictionStrategies.Attributes;
using JackpotPlot.Domain.ValueObjects;

namespace JackpotPlot.Domain.Services.PredictionStrategies;

[PredictionStrategyDescription(PredictionStrategyType.ReducedNumberPool, "Narrows the candidate pool by excluding numbers that appear infrequently, thereby focusing predictions on historically more likely numbers.")]
public class ReducedNumberPoolPredictionStrategy : IPredictionStrategy
{
    private readonly ILotteryConfigurationRepository _lotteryConfigurationRepository;
    private readonly ILotteryHistoryRepository _lotteryHistoryRepository;

    public ReducedNumberPoolPredictionStrategy(ILotteryConfigurationRepository lotteryConfigurationRepository, ILotteryHistoryRepository lotteryHistoryRepository)
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


        // Step 3: Analyze historical data to reduce the number pool
        var reducedPool = AnalyzeReducedNumberPool(historicalDraws, lotteryConfiguration.MainNumbersRange);

        // Step 4: Generate predictions from the reduced pool
        var predictedNumbers = GeneratePredictionsFromReducedPool(reducedPool, lotteryConfiguration.MainNumbersCount);

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
            CalculateReducedPoolConfidence(historicalDraws, predictedNumbers),
            PredictionStrategyType.ReducedNumberPool
        );

        return Result<PredictionResult>.Success(predictionResult);
    }

    public bool Handles(string strategy)
    {
        return strategy.Equals(PredictionStrategyType.ReducedNumberPool, StringComparison.OrdinalIgnoreCase);
    }

    #region Private Helpers

    private static List<int> AnalyzeReducedNumberPool(ICollection<HistoricalDraw> historicalDraws, int numberRange)
    {
        var numberOccurrences = new Dictionary<int, int>();

        // Initialize occurrence counts
        for (int i = 1; i <= numberRange; i++)
        {
            numberOccurrences[i] = 0;
        }

        // Count occurrences of each number in historical data
        foreach (var draw in historicalDraws)
        {
            foreach (var number in draw.WinningNumbers)
            {
                numberOccurrences[number]++;
            }
        }

        // Exclude numbers that have not appeared or appeared infrequently
        var threshold = 0.1 * historicalDraws.Count; // Example: Numbers appearing in less than 10% of draws are excluded
        return numberOccurrences
            .Where(kv => kv.Value >= threshold)
            .Select(kv => kv.Key)
            .ToList();
    }

    private static List<int> GeneratePredictionsFromReducedPool(List<int> reducedPool, int count)
    {
        var random = new Random();
        return reducedPool.OrderBy(_ => random.Next()).Take(count).ToList();
    }

    private static double CalculateReducedPoolConfidence(ICollection<HistoricalDraw> historicalDraws, List<int> predictedNumbers)
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