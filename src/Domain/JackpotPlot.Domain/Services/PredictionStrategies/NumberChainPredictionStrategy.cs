using System.Collections.Immutable;
using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Interfaces;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Repositories;
using JackpotPlot.Domain.ValueObjects;

namespace JackpotPlot.Domain.Services.PredictionStrategies;

public class NumberChainPredictionStrategy : IPredictionStrategy
{
    private readonly ILotteryConfigurationRepository _lotteryConfigurationRepository;
    private readonly ILotteryHistoryRepository _lotteryHistoryRepository;

    public NumberChainPredictionStrategy(ILotteryConfigurationRepository lotteryConfigurationRepository, ILotteryHistoryRepository lotteryHistoryRepository)
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

        // Step 3: Analyze number chains
        var numberChains = AnalyzeNumberChains(historicalDraws);

        // Step 4: Generate predictions from the most frequent chains
        var predictedNumbers = GenerateNumbersFromChains(numberChains, lotteryConfiguration.MainNumbersCount);

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
            CalculateChainConfidence(historicalDraws, predictedNumbers, numberChains),
            PredictionStrategyType.NumberChain
        );

        return Result<PredictionResult>.Success(predictionResult);
    }

    public bool Handles(string strategy)
    {
        return strategy.Equals(PredictionStrategyType.NumberChain, StringComparison.OrdinalIgnoreCase);
    }

    #region Private Helpers

    private static Dictionary<HashSet<int>, int> AnalyzeNumberChains(ICollection<HistoricalDraw> historicalDraws)
    {
        var chainFrequency = new Dictionary<HashSet<int>, int>(HashSet<int>.CreateSetComparer());

        foreach (var draw in historicalDraws)
        {
            var numbers = draw.WinningNumbers;

            // Generate all possible pairs and groups of 3 from the draw
            var chains = GenerateChains(numbers);

            foreach (var chain in chains)
            {
                if (!chainFrequency.ContainsKey(chain))
                {
                    chainFrequency[chain] = 0;
                }
                chainFrequency[chain]++;
            }
        }

        return chainFrequency.OrderByDescending(kv => kv.Value)
                             .ToDictionary(kv => kv.Key, kv => kv.Value);
    }

    private static List<int> GenerateNumbersFromChains(Dictionary<HashSet<int>, int> numberChains, int count)
    {
        var selectedNumbers = new HashSet<int>();

        foreach (var chain in numberChains.Keys)
        {
            foreach (var number in chain)
            {
                if (selectedNumbers.Count < count)
                {
                    selectedNumbers.Add(number);
                }
            }

            if (selectedNumbers.Count >= count)
            {
                break;
            }
        }

        return selectedNumbers.OrderBy(_ => Guid.NewGuid()).ToList(); // Shuffle for randomness
    }

    private static double CalculateChainConfidence(ICollection<HistoricalDraw> historicalDraws, List<int> predictedNumbers, Dictionary<HashSet<int>, int> numberChains)
    {
        int chainMatchCount = 0;
        int numberMatchCount = 0;
        var predictedSet = new HashSet<int>(predictedNumbers);

        // Step 1: Check how many historical chains align with predictions
        foreach (var chain in numberChains.Keys)
        {
            if (chain.IsSubsetOf(predictedSet))
            {
                chainMatchCount++;
            }
        }

        // Step 2: Check how many predicted numbers appear in historical draws
        foreach (var draw in historicalDraws)
        {
            numberMatchCount += draw.WinningNumbers.Intersect(predictedSet).Count();
        }

        // Normalize scores
        double chainConfidence = (double)chainMatchCount / numberChains.Count;
        double numberConfidence = (double)numberMatchCount / (historicalDraws.Count * predictedNumbers.Count);

        // Combine the two confidence metrics
        return (chainConfidence + numberConfidence) / 2.0; // Weighted equally
    }



    private static List<HashSet<int>> GenerateChains(List<int> numbers)
    {
        var chains = new List<HashSet<int>>();

        // Generate pairs
        for (int i = 0; i < numbers.Count; i++)
        {
            for (int j = i + 1; j < numbers.Count; j++)
            {
                chains.Add(new HashSet<int> { numbers[i], numbers[j] });

                // Generate triplets
                for (int k = j + 1; k < numbers.Count; k++)
                {
                    chains.Add(new HashSet<int> { numbers[i], numbers[j], numbers[k] });
                }
            }
        }

        return chains;
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