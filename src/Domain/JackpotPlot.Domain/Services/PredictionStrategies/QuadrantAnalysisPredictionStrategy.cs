using System.Collections.Immutable;
using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Interfaces;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Repositories;
using JackpotPlot.Domain.ValueObjects;

namespace JackpotPlot.Domain.Services.PredictionStrategies;

public class QuadrantAnalysisPredictionStrategy : IPredictionStrategy
{
    private readonly ILotteryConfigurationRepository _lotteryConfigurationRepository;
    private readonly ILotteryHistoryRepository _lotteryHistoryRepository;

    public QuadrantAnalysisPredictionStrategy(ILotteryConfigurationRepository lotteryConfigurationRepository, ILotteryHistoryRepository lotteryHistoryRepository)
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


        // Step 3: Divide the number range into quadrants
        var quadrants = DivideIntoQuadrants(lotteryConfiguration.MainNumbersRange, 4);

        // Step 3: Analyze quadrant frequencies
        var quadrantFrequencies = AnalyzeQuadrantFrequencies(historicalDraws, quadrants);

        // Step 4: Generate predictions based on quadrant frequencies
        var predictedNumbers = GenerateNumbersFromQuadrants(quadrants, quadrantFrequencies, lotteryConfiguration.MainNumbersCount);

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
            CalculateQuadrantConfidence(historicalDraws, predictedNumbers, quadrants),
            PredictionStrategyType.QuadrantAnalysis
        );

        return Result<PredictionResult>.Success(predictionResult);
    }

    public bool Handles(string strategy)
    {
        return strategy.Equals(PredictionStrategyType.QuadrantAnalysis, StringComparison.OrdinalIgnoreCase);
    }

    #region Private Helpers

    private static List<(int start, int end)> DivideIntoQuadrants(int maxRange, int quadrantCount)
    {
        int quadrantSize = maxRange / quadrantCount;
        var quadrants = new List<(int start, int end)>();

        for (int i = 0; i < quadrantCount; i++)
        {
            int start = i * quadrantSize + 1;
            int end = (i == quadrantCount - 1) ? maxRange : (i + 1) * quadrantSize;
            quadrants.Add((start, end));
        }

        return quadrants;
    }

    private static Dictionary<(int start, int end), int> AnalyzeQuadrantFrequencies(ICollection<HistoricalDraw> historicalDraws, List<(int start, int end)> quadrants)
    {
        var frequencies = quadrants.ToDictionary(q => q, _ => 0);

        foreach (var draw in historicalDraws)
        {
            foreach (var number in draw.WinningNumbers)
            {
                var quadrant = quadrants.FirstOrDefault(q => number >= q.start && number <= q.end);
                if (quadrant != default)
                {
                    frequencies[quadrant]++;
                }
            }
        }

        return frequencies;
    }

    private static List<int> GenerateNumbersFromQuadrants(List<(int start, int end)> quadrants, Dictionary<(int start, int end), int> frequencies, int count)
    {
        var random = new Random();
        var selectedNumbers = new List<int>();
        int totalFrequency = frequencies.Values.Sum();

        foreach (var quadrant in quadrants)
        {
            // Determine the number of predictions to select from this quadrant
            int numbersFromQuadrant = (int)Math.Round((double)frequencies[quadrant] / totalFrequency * count);

            var availableNumbers = Enumerable.Range(quadrant.start, quadrant.end - quadrant.start + 1).ToList();
            selectedNumbers.AddRange(availableNumbers.OrderBy(_ => random.Next()).Take(numbersFromQuadrant));

            if (selectedNumbers.Count >= count) break; // Stop if we've selected enough numbers
        }

        // Fill any remaining slots with random selections
        while (selectedNumbers.Count < count)
        {
            int fillerNumber = random.Next(1, quadrants.Last().end + 1);
            if (!selectedNumbers.Contains(fillerNumber)) selectedNumbers.Add(fillerNumber);
        }

        return selectedNumbers.OrderBy(_ => random.Next()).ToList();
    }

    private static double CalculateQuadrantConfidence(ICollection<HistoricalDraw> historicalDraws, List<int> predictedNumbers, List<(int start, int end)> quadrants)
    {
        var historicalQuadrantCounts = AnalyzeQuadrantFrequencies(historicalDraws, quadrants);
        var predictedQuadrantCounts = new Dictionary<(int start, int end), int>();

        foreach (var quadrant in quadrants)
        {
            predictedQuadrantCounts[quadrant] = predictedNumbers.Count(n => n >= quadrant.start && n <= quadrant.end);
        }

        // Compare predicted distribution to historical distribution
        double totalDifference = 0;
        foreach (var quadrant in quadrants)
        {
            totalDifference += Math.Abs(predictedQuadrantCounts[quadrant] - historicalQuadrantCounts[quadrant]);
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