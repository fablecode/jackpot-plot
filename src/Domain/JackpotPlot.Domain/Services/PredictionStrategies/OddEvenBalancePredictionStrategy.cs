using System.Collections.Immutable;
using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Interfaces;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Repositories;
using JackpotPlot.Domain.Services.PredictionStrategies.Attributes;
using JackpotPlot.Domain.ValueObjects;

namespace JackpotPlot.Domain.Services.PredictionStrategies;

[PredictionStrategyDescription(PredictionStrategyType.OddEvenBalance, "Balances predictions by ensuring a similar proportion of odd and even numbers as observed in historical draws.")]
public class OddEvenBalancePredictionStrategy : IPredictionStrategy
{
    private readonly ILotteryConfigurationRepository _lotteryConfigurationRepository;
    private readonly ILotteryHistoryRepository _lotteryHistoryRepository;

    public OddEvenBalancePredictionStrategy(ILotteryConfigurationRepository lotteryConfigurationRepository, ILotteryHistoryRepository lotteryHistoryRepository)
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


        // Step 3: Calculate the odd-even ratio from historical data
        var (oddRatio, evenRatio) = CalculateOddEvenRatio(historicalDraws);

        // Step 4: Determine the count of odd and even numbers to select
        int oddCount = (int)Math.Round(lotteryConfiguration.MainNumbersCount * oddRatio);
        int evenCount = lotteryConfiguration.MainNumbersCount - oddCount; // Remaining numbers are even

        // Step 5: Generate odd and even numbers within the allowed range
        var random = new Random();
        var oddNumbers = GenerateNumbers(1, lotteryConfiguration.MainNumbersRange, n => n % 2 != 0, oddCount, random);
        var evenNumbers = GenerateNumbers(1, lotteryConfiguration.MainNumbersRange, n => n % 2 == 0, evenCount, random);

        // Step 6: Combine and shuffle the numbers
        var predictedNumbers = oddNumbers.Concat(evenNumbers).OrderBy(_ => random.Next()).ToList();

        // Step 7: Predict bonus numbers (optional)
        var bonusNumbers = lotteryConfiguration.BonusNumbersCount > 0
            ? GenerateNumbers(1, lotteryConfiguration.BonusNumbersRange, _ => true, lotteryConfiguration.BonusNumbersCount, random)
            : ImmutableArray<int>.Empty;

        var predictionResult = new PredictionResult
        (
            lotteryId,
            predictedNumbers.ToImmutableArray(),
            bonusNumbers,
            CalculateOddEvenBalanceConfidence(historicalDraws, predictedNumbers), // Example confidence score
            PredictionStrategyType.StatisticalAveraging
        );

        return Result<PredictionResult>.Success(predictionResult);
    }

    public bool Handles(string strategy)
    {
        return !string.IsNullOrWhiteSpace(strategy) && strategy.Equals(PredictionStrategyType.OddEvenBalance, StringComparison.OrdinalIgnoreCase);
    }

    #region Private Helpers

    private static (double oddRatio, double evenRatio) CalculateOddEvenRatio(ICollection<HistoricalDraw> historicalDraws)
    {
        var oddCount = 0;
        var evenCount = 0;

        foreach (var draw in historicalDraws)
        {
            oddCount += draw.WinningNumbers.Count(n => n % 2 != 0); // Count odd numbers
            evenCount += draw.WinningNumbers.Count(n => n % 2 == 0); // Count even numbers
        }

        double total = oddCount + evenCount;
        return (oddCount / total, evenCount / total); // Return both ratios
    }

    private static ImmutableArray<int> GenerateNumbers(int min, int max, Func<int, bool> predicate, int count, Random random)
    {
        var validNumbers = Enumerable.Range(min, max - min + 1).Where(predicate).ToList();
        return validNumbers.OrderBy(_ => random.Next()).Take(count).ToImmutableArray();
    }

    private double CalculateOddEvenBalanceConfidence(ICollection<HistoricalDraw> historicalDraws, List<int> predictedNumbers)
    {
        var predictedOddCount = predictedNumbers.Count(n => n % 2 != 0);
        var predictedEvenCount = predictedNumbers.Count - predictedOddCount;

        int matchCount = 0;

        foreach (var draw in historicalDraws)
        {
            var actualOddCount = draw.WinningNumbers.Count(n => n % 2 != 0);
            var actualEvenCount = draw.WinningNumbers.Count - actualOddCount;

            if (actualOddCount == predictedOddCount && actualEvenCount == predictedEvenCount)
            {
                matchCount++;
            }
        }

        return (double)matchCount / historicalDraws.Count;
    }


    #endregion
}