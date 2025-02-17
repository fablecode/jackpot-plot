using System.Collections.Immutable;
using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Interfaces;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Repositories;
using JackpotPlot.Domain.Services.PredictionStrategies.Attributes;
using JackpotPlot.Domain.ValueObjects;

namespace JackpotPlot.Domain.Services.PredictionStrategies;

[PredictionStrategyDescription(PredictionStrategyType.PatternMatching, "Identifies recurring patterns (such as odd/even or high/low sequences) in historical data and uses these patterns as templates for future predictions.")]
public class PatternMatchingPredictionStrategy : IPredictionStrategy
{
    private readonly ILotteryConfigurationRepository _lotteryConfigurationRepository;
    private readonly ILotteryHistoryRepository _lotteryHistoryRepository;

    public PatternMatchingPredictionStrategy(ILotteryConfigurationRepository lotteryConfigurationRepository, ILotteryHistoryRepository lotteryHistoryRepository)
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

        // Step 4: Analyze historical patterns
        var patterns = AnalyzePatterns(historicalDraws, lotteryConfiguration);

        // Step 5: Select the most frequent pattern
        var selectedPattern = SelectMostFrequentPattern(patterns);

        // Step 6: Generate predictions based on the pattern
        var predictedNumbers = GenerateNumbersFromPattern(selectedPattern, lotteryConfiguration.MainNumbersRange);

        // Step 7: Generate random bonus numbers (if applicable)
        var random = new Random();
        var bonusNumbers = lotteryConfiguration.BonusNumbersCount > 0
            ? GenerateRandomNumbers(1, lotteryConfiguration.BonusNumbersRange, new List<int>(), lotteryConfiguration.BonusNumbersCount, random)
            : ImmutableArray<int>.Empty;

        var predictionResult = new PredictionResult
        (
            lotteryId,
            predictedNumbers.ToImmutableArray(),
            bonusNumbers,
            CalculatePatternMatchingConfidence(historicalDraws, selectedPattern), // Example confidence score
            PredictionStrategyType.PatternMatching
        );
        return Result<PredictionResult>.Success(predictionResult);
    }

    public bool Handles(string strategy)
    {
        return strategy.Equals(PredictionStrategyType.PatternMatching, StringComparison.OrdinalIgnoreCase);
    }

    #region Private Helpers

    private static Dictionary<string, int> AnalyzePatterns(ICollection<HistoricalDraw> historicalDraws, LotteryConfigurationDomain config)
    {
        var patterns = new Dictionary<string, int>();

        foreach (var draw in historicalDraws)
        {
            // Step 1: Validate draw numbers against the configuration
            if (draw.WinningNumbers.Count != config.MainNumbersCount) continue;

            // Step 2: Generate a pattern based on odd/even and high/low rules
            var pattern = string.Join(",", draw.WinningNumbers.Select(n =>
            {
                var oddEven = n % 2 == 0 ? "E" : "O"; // Odd/Even
                var highLow = n <= config.MainNumbersRange / 2 ? "L" : "H"; // High/Low
                return $"{oddEven}{highLow}";
            }));

            // Step 3: Track the frequency of each pattern
            if (!patterns.TryAdd(pattern, 1))
                patterns[pattern]++;
        }

        return patterns;
    }

    private static string SelectMostFrequentPattern(Dictionary<string, int> patterns)
    {
        return patterns.OrderByDescending(p => p.Value).FirstOrDefault().Key;
    }

    private static List<int> GenerateNumbersFromPattern(string pattern, int maxRange)
    {
        var random = new Random();
        var numbers = new List<int>();
        var usedNumbers = new HashSet<int>();

        foreach (var token in pattern.Split(','))
        {
            int number;
            do
            {
                number = random.Next(1, maxRange + 1);
            }
            while (usedNumbers.Contains(number) ||
                   (token.Contains("E") && number % 2 != 0) || // Match Even
                   (token.Contains("O") && number % 2 != 1) || // Match Odd
                   (token.Contains("H") && number <= maxRange / 2) || // Match High
                   (token.Contains("L") && number > maxRange / 2));  // Match Low

            usedNumbers.Add(number);
            numbers.Add(number);
        }

        return numbers;
    }

    private static ImmutableArray<int> GenerateRandomNumbers(int min, int max, List<int> exclude, int count, Random random)
    {
        var availableNumbers = Enumerable.Range(min, max - min + 1).Except(exclude).ToList();
        return availableNumbers.OrderBy(_ => random.Next()).Take(count).ToImmutableArray();
    }

    private double CalculatePatternMatchingConfidence(ICollection<HistoricalDraw> historicalDraws, string predictedPattern)
    {
        int matchCount = 0;

        foreach (var draw in historicalDraws)
        {
            var actualPattern = string.Join(",", draw.WinningNumbers.Select(n => n % 2 == 0 ? "E" : "O"));
            if (actualPattern == predictedPattern)
            {
                matchCount++;
            }
        }

        return (double)matchCount / historicalDraws.Count;
    }


    #endregion
}