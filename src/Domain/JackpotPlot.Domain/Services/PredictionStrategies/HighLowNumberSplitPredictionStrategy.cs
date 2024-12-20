using System.Collections.Immutable;
using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Interfaces;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Repositories;
using JackpotPlot.Domain.ValueObjects;

namespace JackpotPlot.Domain.Services.PredictionStrategies;

public class HighLowNumberSplitPredictionStrategy : IPredictionStrategy
{
    private readonly ILotteryConfigurationRepository _lotteryConfigurationRepository;
    private readonly ILotteryHistoryRepository _lotteryHistoryRepository;

    public HighLowNumberSplitPredictionStrategy(ILotteryConfigurationRepository lotteryConfigurationRepository, ILotteryHistoryRepository lotteryHistoryRepository)
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


        // Step 1: Analyze historical high-low split
        var (lowRatio, highRatio) = AnalyzeHighLowSplit(historicalDraws, lotteryConfiguration.MainNumbersRange);

        // Step 2: Calculate count of low and high numbers to predict
        var lowCount = (int)Math.Round(lotteryConfiguration.MainNumbersCount * lowRatio);
        var highCount = lotteryConfiguration.MainNumbersCount - lowCount;

        // Step 3: Generate predictions
        var random = new Random();
        var lowNumbers = GenerateRandomNumbers(1, lotteryConfiguration.MainNumbersRange / 2, lowCount, random);
        var highNumbers = GenerateRandomNumbers(lotteryConfiguration.MainNumbersRange / 2 + 1, lotteryConfiguration.MainNumbersRange, highCount, random);

        // Combine low and high numbers and shuffle
        var predictedNumbers = lowNumbers.Concat(highNumbers).OrderBy(_ => random.Next()).ToImmutableArray();

        // Step 4: Generate random bonus numbers (if applicable)
        var bonusNumbers = lotteryConfiguration.BonusNumbersCount > 0
            ? GenerateRandomNumbers(1, lotteryConfiguration.BonusNumbersRange, lotteryConfiguration.BonusNumbersCount, random)
            : ImmutableArray<int>.Empty;

        var predictionResult = new PredictionResult
        (
            lotteryId,
            predictedNumbers,
            bonusNumbers,
            0.85, // Example confidence score
            PredictionStrategyType.HighLowNumberSplit
        );

        return Result<PredictionResult>.Success(predictionResult);
    }

    public bool Handles(string strategy)
    {
        return strategy.Equals(PredictionStrategyType.HighLowNumberSplit, StringComparison.OrdinalIgnoreCase);
    }

    #region Private Helpers

    private static (double lowRatio, double highRatio) AnalyzeHighLowSplit(ICollection<HistoricalDraw> historicalDraws, int numberRange)
    {
        int lowCount = 0;
        int highCount = 0;
        int midPoint = numberRange / 2;

        foreach (var draw in historicalDraws)
        {
            foreach (var number in draw.WinningNumbers)
            {
                if (number <= midPoint) lowCount++;
                else highCount++;
            }
        }

        double total = lowCount + highCount;
        return (lowCount / total, highCount / total); // Low and high ratios
    }

    private static ImmutableArray<int> GenerateRandomNumbers(int min, int max, int count, Random random)
    {
        return Enumerable.Range(min, max - min + 1)
            .OrderBy(_ => random.Next())
            .Take(count)
            .ToImmutableArray();
    }

    #endregion
}