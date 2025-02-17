using System.Collections.Immutable;
using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Interfaces;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Repositories;
using JackpotPlot.Domain.Services.PredictionStrategies.Attributes;
using JackpotPlot.Domain.ValueObjects;

namespace JackpotPlot.Domain.Services.PredictionStrategies;

[PredictionStrategyDescription(PredictionStrategyType.SymmetryAnalysis, "Evaluates the balance between high/low and odd/even numbers in historical draws to produce predictions that maintain a symmetric or balanced distribution reflective of past trends.")]
public class SymmetryAnalysisPredictionStrategy : IPredictionStrategy
{
    private readonly ILotteryConfigurationRepository _lotteryConfigurationRepository;
    private readonly ILotteryHistoryRepository _lotteryHistoryRepository;

    public SymmetryAnalysisPredictionStrategy(ILotteryConfigurationRepository lotteryConfigurationRepository, ILotteryHistoryRepository lotteryHistoryRepository)
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

        // Step 3: Analyze historical symmetry
        var symmetryMetrics = AnalyzeSymmetryMetrics(historicalDraws, lotteryConfiguration.MainNumbersRange);

        // Step 4: Generate predictions ensuring symmetry
        var predictedNumbers = GenerateSymmetricNumbers(symmetryMetrics, lotteryConfiguration.MainNumbersCount, lotteryConfiguration.MainNumbersRange);

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
            CalculateSymmetryConfidence(historicalDraws, predictedNumbers, symmetryMetrics),
            PredictionStrategyType.SymmetryAnalysis
        );

        return Result<PredictionResult>.Success(predictionResult);
    }

    public bool Handles(string strategy)
    {
        return strategy.Equals(PredictionStrategyType.SymmetryAnalysis, StringComparison.OrdinalIgnoreCase);
    }

    #region Private Helpers

    private (double highLowRatio, double oddEvenRatio) AnalyzeSymmetryMetrics(ICollection<HistoricalDraw> historicalDraws, int numberRange)
    {
        int highCount = 0, lowCount = 0, oddCount = 0, evenCount = 0;
        int midPoint = numberRange / 2;

        foreach (var draw in historicalDraws)
        {
            foreach (var number in draw.WinningNumbers)
            {
                if (number > midPoint) highCount++; else lowCount++;
                if (number % 2 == 0) evenCount++; else oddCount++;
            }
        }

        // Future enhancement: Use totalNumbers for normalizing ratios if needed.
        //double totalNumbers = historicalDraws.Sum(draw => draw.WinningNumbers.Count);
        return (highLowRatio: (double)highCount / lowCount, oddEvenRatio: (double)oddCount / evenCount);
    }

    private static List<int> GenerateSymmetricNumbers((double highLowRatio, double oddEvenRatio) symmetryMetrics, int count, int numberRange)
    {
        var random = new Random();
        var numbers = new List<int>();
        int midPoint = numberRange / 2;

        int highNumbersNeeded = (int)Math.Round(count * (symmetryMetrics.highLowRatio / (1 + symmetryMetrics.highLowRatio)));
        int lowNumbersNeeded = count - highNumbersNeeded;

        int oddNumbersNeeded = (int)Math.Round(count * (symmetryMetrics.oddEvenRatio / (1 + symmetryMetrics.oddEvenRatio)));
        int evenNumbersNeeded = count - oddNumbersNeeded;

        // Generate high numbers
        numbers.AddRange(GenerateRandomNumbers(midPoint + 1, numberRange, highNumbersNeeded, random));

        // Generate low numbers
        numbers.AddRange(GenerateRandomNumbers(1, midPoint, lowNumbersNeeded, random));

        // Ensure odd/even balance
        numbers = numbers
            .OrderBy(_ => random.Next()) // Shuffle for randomness
            .Take(oddNumbersNeeded)
            .Concat(numbers.Where(n => n % 2 == 0).Take(evenNumbersNeeded))
            .Distinct()
            .Take(count)
            .ToList();

        return numbers.OrderBy(_ => random.Next()).ToList(); // Final shuffle for randomness
    }

    private static double CalculateSymmetryConfidence(ICollection<HistoricalDraw> historicalDraws, List<int> predictedNumbers, (double highLowRatio, double oddEvenRatio) symmetryMetrics)
    {
        int highCount = predictedNumbers.Count(n => n > (predictedNumbers.Max() / 2));
        int lowCount = predictedNumbers.Count(n => n <= (predictedNumbers.Max() / 2));
        int oddCount = predictedNumbers.Count(n => n % 2 != 0);
        int evenCount = predictedNumbers.Count(n => n % 2 == 0);

        double predictedHighLowRatio = highCount == 0 ? 0 : (double)highCount / lowCount;
        double predictedOddEvenRatio = evenCount == 0 ? 0 : (double)oddCount / evenCount;

        double highLowError = Math.Abs(predictedHighLowRatio - symmetryMetrics.highLowRatio);
        double oddEvenError = Math.Abs(predictedOddEvenRatio - symmetryMetrics.oddEvenRatio);

        return 1.0 / (1.0 + highLowError + oddEvenError); // Higher confidence for closer matches
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