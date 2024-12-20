using System.Collections.Immutable;
using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Interfaces;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Repositories;
using JackpotPlot.Domain.ValueObjects;

namespace JackpotPlot.Domain.Services.PredictionStrategies;

public class GapAnalysisPredictionStrategy : IPredictionStrategy
{
    private readonly ILotteryConfigurationRepository _lotteryConfigurationRepository;
    private readonly ILotteryHistoryRepository _lotteryHistoryRepository;

    public GapAnalysisPredictionStrategy(ILotteryConfigurationRepository lotteryConfigurationRepository, ILotteryHistoryRepository lotteryHistoryRepository)
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

        // Step 3: Analyze gaps in historical data
        var gapFrequencies = AnalyzeGaps(historicalDraws);

        // Step 4: Select gaps based on historical frequencies
        var selectedGaps = SelectGaps(gapFrequencies, lotteryConfiguration.MainNumbersCount);

        // Step 5: Generate predicted numbers using the selected gaps
        var predictedNumbers = GenerateNumbersFromGaps(lotteryConfiguration.MainNumbersRange, selectedGaps);

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
            CalculateGapAnalysisConfidence(historicalDraws, predictedNumbers),
            PredictionStrategyType.GapAnalysis
        );

        return Result<PredictionResult>.Success(predictionResult);
    }

    public bool Handles(string strategy)
    {
        return strategy.Equals(PredictionStrategyType.GapAnalysis, StringComparison.OrdinalIgnoreCase);
    }

    #region Private Helpers

    private static Dictionary<int, int> AnalyzeGaps(ICollection<HistoricalDraw> historicalDraws)
    {
        var gapFrequencies = new Dictionary<int, int>();

        foreach (var draw in historicalDraws)
        {
            var sortedNumbers = draw.WinningNumbers.OrderBy(n => n).ToList();
            for (int i = 1; i < sortedNumbers.Count; i++)
            {
                int gap = sortedNumbers[i] - sortedNumbers[i - 1];
                if (!gapFrequencies.TryAdd(gap, 1))
                    gapFrequencies[gap]++;
            }
        }

        return gapFrequencies;
    }

    private static List<int> SelectGaps(Dictionary<int, int> gapFrequencies, int count)
    {
        return gapFrequencies
            .OrderByDescending(g => g.Value) // Most frequent gaps first
            .Take(count)
            .Select(g => g.Key)
            .ToList();
    }

    private static List<int> GenerateNumbersFromGaps(int maxRange, List<int> selectedGaps)
    {
        var random = new Random();
        var numbers = new List<int>();
        int currentNumber = random.Next(1, maxRange / 2);

        foreach (var gap in selectedGaps)
        {
            currentNumber += gap;
            if (currentNumber > maxRange) break;

            numbers.Add(currentNumber);
        }

        // If not enough numbers, fill randomly
        while (numbers.Count < selectedGaps.Count)
        {
            int fillerNumber = random.Next(1, maxRange);
            if (!numbers.Contains(fillerNumber)) numbers.Add(fillerNumber);
        }

        return numbers.OrderBy(_ => random.Next()).ToList();
    }

    private static double CalculateGapAnalysisConfidence(ICollection<HistoricalDraw> historicalDraws, List<int> predictedNumbers)
    {
        int correctGaps = 0;
        int totalGaps = 0;

        foreach (var draw in historicalDraws)
        {
            var actualGaps = GetGaps(draw.WinningNumbers);
            var predictedGaps = GetGaps(predictedNumbers);

            correctGaps += actualGaps.Intersect(predictedGaps).Count();
            totalGaps += actualGaps.Count;
        }

        return (double)correctGaps / totalGaps;
    }

    private static List<int> GetGaps(List<int> numbers)
    {
        var gaps = new List<int>();
        var sortedNumbers = numbers.OrderBy(n => n).ToList();
        for (int i = 1; i < sortedNumbers.Count; i++)
        {
            gaps.Add(sortedNumbers[i] - sortedNumbers[i - 1]);
        }
        return gaps;
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