using System.Collections.Immutable;
using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Interfaces;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Repositories;
using JackpotPlot.Domain.Services.PredictionStrategies.Attributes;
using JackpotPlot.Domain.ValueObjects;

namespace JackpotPlot.Domain.Services.PredictionStrategies;

[PredictionStrategyDescription(PredictionStrategyType.InvertedFrequency, "Emphasizes “cold” numbers that have appeared less frequently in the past, under the theory that these may be more likely to be drawn in the future.")]
public class InvertedFrequencyPredictionStrategy : IPredictionStrategy
{
    private readonly ILotteryConfigurationRepository _lotteryConfigurationRepository;
    private readonly ILotteryHistoryRepository _lotteryHistoryRepository;

    public InvertedFrequencyPredictionStrategy(ILotteryConfigurationRepository lotteryConfigurationRepository, ILotteryHistoryRepository lotteryHistoryRepository)
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

        // Step 3: Analyze historical data to find low-frequency numbers
        var invertedFrequencyNumbers = AnalyzeInvertedFrequencies(historicalDraws, lotteryConfiguration.MainNumbersRange);

        // Step 4: Generate predictions from the low-frequency numbers
        var predictedNumbers = GeneratePredictionsFromInvertedFrequencies(invertedFrequencyNumbers, lotteryConfiguration.MainNumbersCount);

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
            CalculateInvertedFrequencyConfidence(historicalDraws, predictedNumbers),
            PredictionStrategyType.InvertedFrequency
        );

        return Result<PredictionResult>.Success(predictionResult);
    }

    public bool Handles(string strategy)
    {
        return !string.IsNullOrWhiteSpace(strategy) && strategy.Equals(PredictionStrategyType.InvertedFrequency, StringComparison.OrdinalIgnoreCase);
    }

    #region Private Helpers

    private static Dictionary<int, int> AnalyzeInvertedFrequencies(ICollection<HistoricalDraw> historicalDraws, int numberRange)
    {
        var frequencyMap = new Dictionary<int, int>();

        // Initialize frequency counts
        for (int i = 1; i <= numberRange; i++)
        {
            frequencyMap[i] = 0;
        }

        // Count occurrences of each number in historical data
        foreach (var draw in historicalDraws)
        {
            foreach (var number in draw.WinningNumbers)
            {
                if (frequencyMap.ContainsKey(number))
                {
                    frequencyMap[number]++;
                }
            }
        }

        // Sort numbers by frequency in ascending order
        return frequencyMap.OrderBy(kv => kv.Value).ToDictionary(kv => kv.Key, kv => kv.Value);
    }

    private static List<int> GeneratePredictionsFromInvertedFrequencies(Dictionary<int, int> invertedFrequencies, int count)
    {
        var random = new Random();
        return invertedFrequencies
            .Keys
            .OrderBy(_ => random.Next()) // Randomize order of low-frequency numbers
            .Take(count) // Take the required number of predictions
            .ToList();
    }

    private static double CalculateInvertedFrequencyConfidence(ICollection<HistoricalDraw> historicalDraws, List<int> predictedNumbers)
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