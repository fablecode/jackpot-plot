using System.Collections.Immutable;
using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Interfaces;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Repositories;
using JackpotPlot.Domain.ValueObjects;

namespace JackpotPlot.Domain.Services.PredictionStrategies;

public class RarePatternsPredictionStrategy : IPredictionStrategy
{
    private readonly ILotteryConfigurationRepository _lotteryConfigurationRepository;
    private readonly ILotteryHistoryRepository _lotteryHistoryRepository;

    public RarePatternsPredictionStrategy(ILotteryConfigurationRepository lotteryConfigurationRepository, ILotteryHistoryRepository lotteryHistoryRepository)
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

        // Step 3: Analyze historical patterns
        var rarePatterns = AnalyzeRarePatterns(historicalDraws, lotteryConfiguration.MainNumbersRange);

        // Step 4: Generate predictions based on rare patterns
        var predictedNumbers = GeneratePredictionsFromRarePatterns(rarePatterns, lotteryConfiguration.MainNumbersCount, lotteryConfiguration.MainNumbersRange);

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
            CalculateRarePatternsConfidence(historicalDraws, predictedNumbers, rarePatterns),
            PredictionStrategyType.RarePatterns
        );

        return Result<PredictionResult>.Success(predictionResult);
    }

    public bool Handles(string strategy)
    {
        return strategy.Equals(PredictionStrategyType.RarePatterns, StringComparison.OrdinalIgnoreCase);
    }

    #region Private Helpers

    private static Dictionary<string, int> AnalyzeRarePatterns(ICollection<HistoricalDraw> historicalDraws, int numberRange)
    {
        var patternFrequency = new Dictionary<string, int>();

        foreach (var draw in historicalDraws)
        {
            // Generate a pattern string (e.g., "2L3H" for 2 low, 3 high numbers)
            string pattern = GeneratePattern(draw.WinningNumbers, numberRange);

            if (!patternFrequency.ContainsKey(pattern))
            {
                patternFrequency[pattern] = 0;
            }

            patternFrequency[pattern]++;
        }

        // Sort patterns by rarity (ascending frequency)
        return patternFrequency.OrderBy(kv => kv.Value).ToDictionary(kv => kv.Key, kv => kv.Value);
    }

    private static string GeneratePattern(List<int> numbers, int numberRange)
    {
        int midPoint = numberRange / 2;
        int lowCount = numbers.Count(n => n <= midPoint);
        int highCount = numbers.Count - lowCount;

        int oddCount = numbers.Count(n => n % 2 != 0);
        int evenCount = numbers.Count - oddCount;

        return $"{lowCount}L{highCount}H-{oddCount}O{evenCount}E";
    }

    private static List<int> GeneratePredictionsFromRarePatterns(Dictionary<string, int> rarePatterns, int count, int numberRange)
    {
        var random = new Random();
        var predictedNumbers = new List<int>();

        // Use the rarest pattern
        var rarestPattern = rarePatterns.Keys.FirstOrDefault();
        if (rarestPattern != null)
        {
            // Parse the pattern to determine the number distribution
            var parts = rarestPattern.Split('-');
            var lowHigh = parts[0].Split(new[] { 'L', 'H' }, StringSplitOptions.RemoveEmptyEntries);
            var oddEven = parts[1].Split(new[] { 'O', 'E' }, StringSplitOptions.RemoveEmptyEntries);

            int lowCount = int.Parse(lowHigh[0]);
            int highCount = int.Parse(lowHigh[1]);
            int oddCount = int.Parse(oddEven[0]);
            int evenCount = int.Parse(oddEven[1]);

            // Generate low numbers
            var lowNumbers = GenerateRandomNumbers(1, numberRange / 2, lowCount, random);

            // Generate high numbers
            var highNumbers = GenerateRandomNumbers((numberRange / 2) + 1, numberRange, highCount, random);

            // Merge low and high numbers
            var allNumbers = lowNumbers.Concat(highNumbers).ToList();

            // Balance odd/even distribution
            var oddNumbers = allNumbers.Where(n => n % 2 != 0).Take(oddCount).ToList();
            var evenNumbers = allNumbers.Where(n => n % 2 == 0).Take(evenCount).ToList();

            predictedNumbers = oddNumbers.Concat(evenNumbers).ToList();
        }

        // If not enough numbers, fill randomly
        while (predictedNumbers.Count < count)
        {
            int fillerNumber = random.Next(1, numberRange + 1);
            if (!predictedNumbers.Contains(fillerNumber))
            {
                predictedNumbers.Add(fillerNumber);
            }
        }

        return predictedNumbers.OrderBy(_ => random.Next()).Take(count).ToList();
    }

    private static double CalculateRarePatternsConfidence(ICollection<HistoricalDraw> historicalDraws, List<int> predictedNumbers, Dictionary<string, int> rarePatterns)
    {
        int matchCount = 0;
        string predictedPattern = GeneratePattern(predictedNumbers, historicalDraws.First().WinningNumbers.Max());

        if (rarePatterns.TryGetValue(predictedPattern, out var pattern))
        {
            matchCount = pattern;
        }

        return 1.0 / (1.0 + matchCount); // Higher confidence for rarer patterns
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