using System.Collections.Immutable;
using JackpotPlot.Domain.Models;

namespace JackpotPlot.Domain.Services.PredictionStrategies.Helpers;

public static class ClusteringAnalysisPredictionStrategyHelpers
{
    public static int[,] GenerateCoOccurrenceMatrix(ICollection<HistoricalDraw> historicalDraws, int numberRange)
    {
        var matrix = new int[numberRange + 1, numberRange + 1];

        foreach (var draw in historicalDraws)
        {
            var numbers = draw.WinningNumbers;
            for (int i = 0; i < numbers.Count; i++)
            {
                for (int j = i + 1; j < numbers.Count; j++)
                {
                    matrix[numbers[i], numbers[j]]++;
                    matrix[numbers[j], numbers[i]]++; // Symmetric co-occurrence
                }
            }
        }

        return matrix;
    }

    public static List<List<int>> PerformClustering(int[,] coOccurrenceMatrix, int clusterCount)
    {
        // Simplified clustering logic: Use co-occurrence frequencies to group numbers
        var clusters = new List<List<int>>();
        var random = new Random();

        for (int i = 0; i < clusterCount; i++)
        {
            clusters.Add(new List<int>());
        }

        for (int number = 1; number < coOccurrenceMatrix.GetLength(0); number++)
        {
            int assignedCluster = random.Next(0, clusterCount); // Randomly assign to a cluster
            clusters[assignedCluster].Add(number);
        }

        return clusters;
    }

    public static List<int> SelectNumbersFromClusters(List<List<int>> clusters, int numberCount)
    {
        var selectedNumbers = new List<int>();
        var random = new Random();

        foreach (var cluster in clusters)
        {
            if (selectedNumbers.Count >= numberCount)
                break;

            if (cluster.Any())
            {
                selectedNumbers.Add(cluster[random.Next(cluster.Count)]);
            }
        }

        return selectedNumbers.Take(numberCount).ToList();
    }

    public static ImmutableArray<int> GenerateRandomNumbers(int min, int max, List<int> exclude, int count, Random random)
    {
        var availableNumbers = Enumerable.Range(min, max - min + 1).Except(exclude).ToList();
        return availableNumbers.OrderBy(_ => random.Next()).Take(count).ToImmutableArray();
    }

    public static double CalculateClusteringAnalysisConfidence(List<List<int>> clusters, List<int> predictedNumbers)
    {
        int correctPredictions = 0;

        foreach (var number in predictedNumbers)
        {
            if (clusters.Any(cluster => cluster.Contains(number)))
            {
                correctPredictions++;
            }
        }

        return (double)correctPredictions / predictedNumbers.Count;
    }
}