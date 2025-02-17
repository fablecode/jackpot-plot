using System.Collections.Immutable;
using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Interfaces;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Repositories;
using JackpotPlot.Domain.Services.PredictionStrategies.Attributes;
using JackpotPlot.Domain.ValueObjects;

namespace JackpotPlot.Domain.Services.PredictionStrategies;

[PredictionStrategyDescription(PredictionStrategyType.SeasonalPatterns, "Analyzes seasonal or temporal trends in historical draws (by month, season, etc.) to predict numbers that are more likely to appear during the current season.")]
public class SeasonalPatternsPredictionStrategy : IPredictionStrategy
{
    private readonly ILotteryConfigurationRepository _lotteryConfigurationRepository;
    private readonly ILotteryHistoryRepository _lotteryHistoryRepository;

    public SeasonalPatternsPredictionStrategy(ILotteryConfigurationRepository lotteryConfigurationRepository, ILotteryHistoryRepository lotteryHistoryRepository)
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


        // Step 1: Determine the current season or month
        var currentSeason = GetCurrentSeason();

        // Step 2: Analyze seasonal patterns from historical data
        var seasonalFrequencies = AnalyzeSeasonalPatterns(historicalDraws, currentSeason, lotteryConfiguration.MainNumbersRange);

        // Step 3: Generate predictions using seasonal frequencies
        var predictedNumbers = GenerateNumbersFromSeasonalFrequencies(seasonalFrequencies, lotteryConfiguration.MainNumbersCount);

        // Step 4: Generate random bonus numbers (if applicable)
        var random = new Random();
        var bonusNumbers = lotteryConfiguration.BonusNumbersCount > 0
            ? GenerateRandomNumbers(1, lotteryConfiguration.BonusNumbersRange, lotteryConfiguration.BonusNumbersCount, random)
            : new List<int>();

        var predictionResult = new PredictionResult
        (
            lotteryId,
            predictedNumbers.ToImmutableArray(),
            bonusNumbers.ToImmutableArray(),
            CalculateSeasonalConfidence(historicalDraws, predictedNumbers, currentSeason),
            "seasonal-patterns"
        );

        return Result<PredictionResult>.Success(predictionResult);
    }

    public bool Handles(string strategy)
    {
        return strategy.Equals(PredictionStrategyType.SeasonalPatterns, StringComparison.OrdinalIgnoreCase);
    }

    #region Private Helpers

    private static string GetCurrentSeason()
    {
        var month = DateTime.Now.Month;

        return month switch
        {
            12 or 1 or 2 => "Winter",
            3 or 4 or 5 => "Spring",
            6 or 7 or 8 => "Summer",
            9 or 10 or 11 => "Fall",
            _ => "Unknown"
        };
    }

    private static Dictionary<int, int> AnalyzeSeasonalPatterns(ICollection<HistoricalDraw> historicalDraws, string season, int numberRange)
    {
        var frequencies = new Dictionary<int, int>();
        for (int i = 1; i <= numberRange; i++)
        {
            frequencies[i] = 0;
        }

        foreach (var draw in historicalDraws)
        {
            var drawSeason = GetSeasonFromDate(draw.DrawDate);

            if (drawSeason == season)
            {
                foreach (var number in draw.WinningNumbers)
                {
                    frequencies[number]++;
                }
            }
        }

        return frequencies;
    }

    private static string GetSeasonFromDate(DateTime date)
    {
        var month = date.Month;

        return month switch
        {
            12 or 1 or 2 => "Winter",
            3 or 4 or 5 => "Spring",
            6 or 7 or 8 => "Summer",
            9 or 10 or 11 => "Fall",
            _ => "Unknown"
        };
    }

    private static List<int> GenerateNumbersFromSeasonalFrequencies(Dictionary<int, int> seasonalFrequencies, int count)
    {
        var random = new Random();
        var selectedNumbers = new List<int>();

        // Order numbers by frequency and randomly select
        foreach (var number in seasonalFrequencies.OrderByDescending(kv => kv.Value).Select(kv => kv.Key))
        {
            if (selectedNumbers.Count < count && !selectedNumbers.Contains(number))
            {
                selectedNumbers.Add(number);
            }
        }

        // Shuffle for randomness
        return selectedNumbers.OrderBy(_ => random.Next()).ToList();
    }

    private static double CalculateSeasonalConfidence(ICollection<HistoricalDraw> historicalDraws, List<int> predictedNumbers, string currentSeason)
    {
        var seasonalDraws = historicalDraws.Where(draw => GetSeasonFromDate(draw.DrawDate) == currentSeason).ToList();
        int matchCount = 0;

        foreach (var draw in seasonalDraws)
        {
            matchCount += draw.WinningNumbers.Intersect(predictedNumbers).Count();
        }

        return (double)matchCount / (seasonalDraws.Count * predictedNumbers.Count);
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