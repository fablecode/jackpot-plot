using System.Collections.Immutable;
using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Interfaces;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Repositories;
using JackpotPlot.Domain.Services.PredictionStrategies.Attributes;
using JackpotPlot.Domain.ValueObjects;

namespace JackpotPlot.Domain.Services.PredictionStrategies;

[PredictionStrategyDescription(PredictionStrategyType.GroupSelection, "Splits the number range into predefined groups (e.g., low, medium, high) and selects numbers proportionally from each group based on historical frequency.")]
public class GroupSelectionPredictionStrategy : IPredictionStrategy
{
    private readonly ILotteryConfigurationRepository _lotteryConfigurationRepository;
    private readonly ILotteryHistoryRepository _lotteryHistoryRepository;

    public GroupSelectionPredictionStrategy(ILotteryConfigurationRepository lotteryConfigurationRepository, ILotteryHistoryRepository lotteryHistoryRepository)
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

        // Step 3: Define groups based on the number range
        var groups = DivideIntoGroups(lotteryConfiguration.MainNumbersRange, 3); // Divide into 3 groups (low, medium, high)

        // Step 4: Analyze historical data to calculate group frequencies
        var groupFrequencies = AnalyzeGroupFrequencies(historicalDraws, groups);

        // Step 5: Generate predictions based on group frequencies
        var predictedNumbers = GenerateNumbersFromGroups(groups, groupFrequencies, lotteryConfiguration.MainNumbersCount);

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
            CalculateGroupConfidence(historicalDraws, predictedNumbers, groups),
            PredictionStrategyType.GroupSelection
        );

        return Result<PredictionResult>.Success(predictionResult);
    }

    public bool Handles(string strategy)
    {
        return !string.IsNullOrWhiteSpace(strategy) && strategy.Equals(PredictionStrategyType.GroupSelection, StringComparison.OrdinalIgnoreCase);
    }

    #region Private Helpers

    private static List<(int start, int end)> DivideIntoGroups(int numberRange, int groupCount)
    {
        var groupSize = numberRange / groupCount;
        var groups = new List<(int start, int end)>();

        for (int i = 0; i < groupCount; i++)
        {
            int start = i * groupSize + 1;
            int end = (i == groupCount - 1) ? numberRange : (i + 1) * groupSize;
            groups.Add((start, end));
        }

        return groups;
    }

    private static Dictionary<(int start, int end), int> AnalyzeGroupFrequencies(ICollection<HistoricalDraw> historicalDraws, List<(int start, int end)> groups)
    {
        var frequencies = groups.ToDictionary(group => group, _ => 0);

        foreach (var draw in historicalDraws)
        {
            foreach (var number in draw.WinningNumbers)
            {
                var group = groups.FirstOrDefault(g => number >= g.start && number <= g.end);
                if (group != default)
                {
                    frequencies[group]++;
                }
            }
        }

        return frequencies;
    }

    private static List<int> GenerateNumbersFromGroups(List<(int start, int end)> groups, Dictionary<(int start, int end), int> groupFrequencies, int count)
    {
        var random = new Random();
        var selectedNumbers = new List<int>();
        int totalFrequency = groupFrequencies.Values.Sum();

        foreach (var group in groups)
        {
            // Calculate the number of predictions to select from this group
            int numbersFromGroup = (int)Math.Round((double)groupFrequencies[group] / totalFrequency * count);

            var availableNumbers = Enumerable.Range(group.start, group.end - group.start + 1).ToList();
            selectedNumbers.AddRange(availableNumbers.OrderBy(_ => random.Next()).Take(numbersFromGroup));

            if (selectedNumbers.Count >= count) break; // Stop if we've selected enough numbers
        }

        // Fill remaining slots if needed
        while (selectedNumbers.Count < count)
        {
            int fillerNumber = random.Next(1, groups.Last().end + 1);
            if (!selectedNumbers.Contains(fillerNumber)) selectedNumbers.Add(fillerNumber);
        }

        return selectedNumbers.OrderBy(_ => random.Next()).ToList();
    }

    private double CalculateGroupConfidence(ICollection<HistoricalDraw> historicalDraws, List<int> predictedNumbers, List<(int start, int end)> groups)
    {
        var historicalGroupCounts = AnalyzeGroupFrequencies(historicalDraws, groups);
        var predictedGroupCounts = new Dictionary<(int start, int end), int>();

        foreach (var group in groups)
        {
            predictedGroupCounts[group] = predictedNumbers.Count(n => n >= group.start && n <= group.end);
        }

        // Compare predicted distribution to historical distribution
        double totalDifference = 0;
        foreach (var group in groups)
        {
            totalDifference += Math.Abs(predictedGroupCounts[group] - historicalGroupCounts[group]);
        }

        return 1.0 / (1.0 + totalDifference); // Higher confidence for closer matches
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