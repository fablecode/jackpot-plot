using System.Collections.Immutable;
using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Interfaces;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Repositories;
using JackpotPlot.Domain.Services.PredictionStrategies.Attributes;
using JackpotPlot.Domain.ValueObjects;

namespace JackpotPlot.Domain.Services.PredictionStrategies;

[PredictionStrategyDescription(PredictionStrategyType.CyclicPatterns, "Identifies recurring cycles or intervals in historical draws and predicts numbers that are due to appear based on these cyclic patterns.")]
public class CyclicPatternsPredictionStrategy : IPredictionStrategy
{
    private readonly ILotteryConfigurationRepository _lotteryConfigurationRepository;
    private readonly ILotteryHistoryRepository _lotteryHistoryRepository;

    public CyclicPatternsPredictionStrategy(ILotteryConfigurationRepository lotteryConfigurationRepository, ILotteryHistoryRepository lotteryHistoryRepository)
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


        // Step 3: Identify cyclic patterns
        var cycles = AnalyzeCyclicPatterns(historicalDraws.ToList(), lotteryConfiguration.MainNumbersRange);

        // Step 4: Predict numbers based on cyclic patterns
        var predictedNumbers = GenerateNumbersFromCycles(cycles, lotteryConfiguration.MainNumbersCount);

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
            CalculateCyclicConfidence(historicalDraws.ToList(), predictedNumbers, cycles),
            PredictionStrategyType.CyclicPatterns
        );

        return Result<PredictionResult>.Success(predictionResult);
    }

    public bool Handles(string strategy)
    {
        return strategy.Equals(PredictionStrategyType.CyclicPatterns, StringComparison.OrdinalIgnoreCase);
    }

    #region Private Helpers

    private static Dictionary<int, List<int>> AnalyzeCyclicPatterns(List<HistoricalDraw> historicalDraws, int numberRange)
    {
        var cycles = new Dictionary<int, List<int>>();

        for (int number = 1; number <= numberRange; number++)
        {
            var appearances = new List<int>();
            int lastAppearance = -1;

            for (int i = 0; i < historicalDraws.Count; i++)
            {
                if (historicalDraws[i].WinningNumbers.Contains(number))
                {
                    if (lastAppearance != -1)
                    {
                        appearances.Add(i - lastAppearance);
                    }
                    lastAppearance = i;
                }
            }

            cycles[number] = appearances;
        }

        return cycles;
    }

    private static List<int> GenerateNumbersFromCycles(Dictionary<int, List<int>> cycles, int count)
    {
        var random = new Random();
        var selectedNumbers = new List<int>();

        // Sort numbers by their average cycle length
        var sortedCycles = cycles
            .Where(c => c.Value.Count > 0) // Exclude numbers with no cycle data
            .OrderBy(c => c.Value.Average())
            .Select(c => c.Key)
            .ToList();

        // Select numbers with the shortest cycles first
        foreach (var number in sortedCycles)
        {
            if (selectedNumbers.Count < count)
            {
                selectedNumbers.Add(number);
            }
        }

        // Shuffle results for randomness
        return selectedNumbers.OrderBy(_ => random.Next()).ToList();
    }

    private static double CalculateCyclicConfidence(List<HistoricalDraw> historicalDraws, List<int> predictedNumbers, Dictionary<int, List<int>> cycles)
    {
        int matchCount = 0;

        foreach (var number in predictedNumbers)
        {
            if (cycles.ContainsKey(number) && cycles[number].Count > 0)
            {
                // Check if the number is due based on its cycle
                int averageCycle = (int)cycles[number].Average();
                int lastAppearance = historicalDraws.FindIndex(draw => draw.WinningNumbers.Contains(number));
                if (lastAppearance != -1 && historicalDraws.Count - lastAppearance >= averageCycle)
                {
                    matchCount++;
                }
            }
        }

        return (double)matchCount / predictedNumbers.Count;
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