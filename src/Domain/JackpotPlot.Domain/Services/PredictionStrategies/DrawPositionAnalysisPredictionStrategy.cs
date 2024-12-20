using System.Collections.Immutable;
using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Interfaces;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Repositories;
using JackpotPlot.Domain.ValueObjects;

namespace JackpotPlot.Domain.Services.PredictionStrategies;

public class DrawPositionAnalysisPredictionStrategy : IPredictionStrategy
{
    private readonly ILotteryConfigurationRepository _lotteryConfigurationRepository;
    private readonly ILotteryHistoryRepository _lotteryHistoryRepository;

    public DrawPositionAnalysisPredictionStrategy(ILotteryConfigurationRepository lotteryConfigurationRepository, ILotteryHistoryRepository lotteryHistoryRepository)
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

        // Step 3: Analyze draw positions
        var positionFrequencies = AnalyzeDrawPositions(historicalDraws, lotteryConfiguration.MainNumbersCount, lotteryConfiguration.MainNumbersRange);

        // Step 4: Generate predictions based on position trends
        var predictedNumbers = GenerateNumbersFromPositions(positionFrequencies);

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
            CalculatePositionConfidence(historicalDraws, predictedNumbers, positionFrequencies),
            PredictionStrategyType.DrawPositionAnalysis
        );

        return Result<PredictionResult>.Success(predictionResult);
    }

    public bool Handles(string strategy)
    {
        return strategy.Equals(PredictionStrategyType.DrawPositionAnalysis, StringComparison.OrdinalIgnoreCase);
    }

    #region Private Helpers

    private static Dictionary<int, Dictionary<int, int>> AnalyzeDrawPositions(ICollection<HistoricalDraw> historicalDraws, int mainNumbersCount, int numberRange)
    {
        var positionFrequencies = new Dictionary<int, Dictionary<int, int>>();

        for (int position = 0; position < mainNumbersCount; position++)
        {
            positionFrequencies[position] = new Dictionary<int, int>();

            for (int number = 1; number <= numberRange; number++)
            {
                positionFrequencies[position][number] = 0;
            }
        }

        foreach (var draw in historicalDraws)
        {
            for (int position = 0; position < mainNumbersCount; position++)
            {
                if (position < draw.WinningNumbers.Count)
                {
                    var number = draw.WinningNumbers[position];
                    positionFrequencies[position][number]++;
                }
            }
        }

        return positionFrequencies;
    }

    private static List<int> GenerateNumbersFromPositions(Dictionary<int, Dictionary<int, int>> positionFrequencies)
    {
        var random = new Random();
        var selectedNumbers = new List<int>();

        foreach (var position in positionFrequencies.Keys)
        {
            // Select the most frequent number for each position
            var mostFrequentNumber = positionFrequencies[position]
                .OrderByDescending(kv => kv.Value)
                .ThenBy(_ => random.Next()) // Break ties randomly
                .First().Key;

            if (!selectedNumbers.Contains(mostFrequentNumber))
            {
                selectedNumbers.Add(mostFrequentNumber);
            }
        }

        return selectedNumbers.OrderBy(_ => random.Next()).ToList(); // Shuffle for randomness
    }

    private static double CalculatePositionConfidence(ICollection<HistoricalDraw> historicalDraws, List<int> predictedNumbers, Dictionary<int, Dictionary<int, int>> positionFrequencies)
    {
        int matchCount = 0;

        foreach (var draw in historicalDraws)
        {
            for (int position = 0; position < draw.WinningNumbers.Count; position++)
            {
                if (predictedNumbers.Count > position && draw.WinningNumbers[position] == predictedNumbers[position])
                {
                    matchCount++;
                }
            }
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