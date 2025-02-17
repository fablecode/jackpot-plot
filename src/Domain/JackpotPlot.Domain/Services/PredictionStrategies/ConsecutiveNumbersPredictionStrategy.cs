using System.Collections.Immutable;
using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Interfaces;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Repositories;
using JackpotPlot.Domain.Services.PredictionStrategies.Attributes;
using JackpotPlot.Domain.ValueObjects;

namespace JackpotPlot.Domain.Services.PredictionStrategies;

[PredictionStrategyDescription(PredictionStrategyType.ConsecutiveNumbers, "Focuses on frequently occurring consecutive pairs or sequences, assuming that numbers appearing in a chain might appear together again.")]
public class ConsecutiveNumbersPredictionStrategy : IPredictionStrategy
{
    private readonly ILotteryConfigurationRepository _lotteryConfigurationRepository;
    private readonly ILotteryHistoryRepository _lotteryHistoryRepository;

    public ConsecutiveNumbersPredictionStrategy(ILotteryConfigurationRepository lotteryConfigurationRepository, ILotteryHistoryRepository lotteryHistoryRepository)
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

        // Step 3: Analyze historical data for consecutive pairs
        var consecutivePairs = FindFrequentConsecutivePairs(historicalDraws);

        // Step 4: Select the most frequent consecutive numbers
        var selectedConsecutiveNumbers = SelectConsecutiveNumbers(consecutivePairs, lotteryConfiguration.MainNumbersCount);

        // Step 5: Fill remaining numbers with random selections
        var remainingNumbersCount = lotteryConfiguration.MainNumbersCount - selectedConsecutiveNumbers.Count;
        var random = new Random();
        var remainingNumbers = GenerateRandomNumbers(
            1, lotteryConfiguration.MainNumbersRange, selectedConsecutiveNumbers, remainingNumbersCount, random
        );

        // Combine consecutive and random numbers
        var predictedNumbers = selectedConsecutiveNumbers.Concat(remainingNumbers).OrderBy(_ => random.Next()).ToList();

        // Step 6: Predict bonus numbers (if applicable)
        var bonusNumbers = lotteryConfiguration.BonusNumbersCount > 0
            ? GenerateRandomNumbers(1, lotteryConfiguration.BonusNumbersRange, new List<int>(), lotteryConfiguration.BonusNumbersCount, random)
            : ImmutableArray<int>.Empty;

        var predictionResult = new PredictionResult
        (
            lotteryId,
            predictedNumbers.ToImmutableArray(),
            bonusNumbers,
            CalculateConsecutiveNumbersConfidence(historicalDraws, predictedNumbers), // Example confidence score
            PredictionStrategyType.ConsecutiveNumbers
        );

        return Result<PredictionResult>.Success(predictionResult);
    }

    public bool Handles(string strategy)
    {
        return strategy.Equals(PredictionStrategyType.ConsecutiveNumbers, StringComparison.OrdinalIgnoreCase);
    }

    #region Private Helpers

    private static Dictionary<(int, int), int> FindFrequentConsecutivePairs(ICollection<HistoricalDraw> historicalDraws)
    {
        var pairCounts = new Dictionary<(int, int), int>();

        foreach (var draw in historicalDraws)
        {
            var sortedNumbers = draw.WinningNumbers.OrderBy(n => n).ToList();
            for (int i = 0; i < sortedNumbers.Count - 1; i++)
            {
                if (sortedNumbers[i + 1] == sortedNumbers[i] + 1) // Consecutive pair
                {
                    var pair = (sortedNumbers[i], sortedNumbers[i + 1]);
                    if (!pairCounts.TryAdd(pair, 1))
                        pairCounts[pair]++;
                }
            }
        }

        return pairCounts;
    }

    private static List<int> SelectConsecutiveNumbers(Dictionary<(int, int), int> consecutivePairs, int maxCount)
    {
        var selectedNumbers = new HashSet<int>();

        foreach (var pair in consecutivePairs.OrderByDescending(p => p.Value).Take(maxCount))
        {
            selectedNumbers.Add(pair.Key.Item1);
            selectedNumbers.Add(pair.Key.Item2);
        }

        return selectedNumbers.Take(maxCount).ToList(); // Limit to maxCount numbers
    }

    private static ImmutableArray<int> GenerateRandomNumbers(int min, int max, List<int> exclude, int count, Random random)
    {
        var availableNumbers = Enumerable.Range(min, max - min + 1).Except(exclude).ToList();
        return availableNumbers.OrderBy(_ => random.Next()).Take(count).ToImmutableArray();
    }

    private double CalculateConsecutiveNumbersConfidence(ICollection<HistoricalDraw> historicalDraws, List<int> predictedNumbers)
    {
        int correctSequences = 0;
        int totalSequences = 0;

        foreach (var draw in historicalDraws)
        {
            var actualConsecutivePairs = GetConsecutivePairs(draw.WinningNumbers);
            var predictedConsecutivePairs = GetConsecutivePairs(predictedNumbers);

            correctSequences += actualConsecutivePairs.Intersect(predictedConsecutivePairs).Count();
            totalSequences += actualConsecutivePairs.Count;
        }

        return (double)correctSequences / totalSequences;
    }

    private static List<(int, int)> GetConsecutivePairs(List<int> numbers)
    {
        var pairs = new List<(int, int)>();
        for (var i = 1; i < numbers.Count; i++)
        {
            if (numbers[i] == numbers[i - 1] + 1)
            {
                pairs.Add((numbers[i - 1], numbers[i]));
            }
        }
        return pairs;
    }

    #endregion
}