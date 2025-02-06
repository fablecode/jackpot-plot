using FluentAssertions;
using JackpotPlot.Domain.Services.PredictionStrategies.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.ServicesTests.HelperTests.ClusteringAnalysisPredictionStrategyHelpersTests;

[TestFixture]
public class SelectNumbersFromClustersTests
{
    [Test]
    public void Given_Clusters_With_Numbers_When_SelectNumbersFromClusters_Method_Is_Invoked_And_Number_Count_Less_Than_Cluster_Count_Then_Return_Exact_Number_Count_Of_Selected_Numbers()
    {
        // Arrange
        var clusters = new List<List<int>>
        {
            new() { 1, 2, 3 },
            new() { 4, 5, 6 },
            new() { 7, 8, 9 },
            new() { 10, 11, 12 }
        };
        const int numberCount = 2; // fewer than the number of clusters

        // Act
        var selectedNumbers = ClusteringAnalysisPredictionStrategyHelpers.SelectNumbersFromClusters(clusters, numberCount);

        // Assert
        selectedNumbers.Should().HaveCount(numberCount);
        // Since clusters are processed in order, the selected number from the first cluster
        // must be one of the numbers in clusters[0] and similarly for clusters[1].
        selectedNumbers[0].Should().BeOneOf(clusters[0]);
        selectedNumbers[1].Should().BeOneOf(clusters[1]);
    }

    [Test]
    public void Given_Clusters_With_Numbers_When_SelectNumbersFromClusters_Method_Is_Invoked_And_Number_Count_Equals_Cluster_Count_Then_Return_One_Number_Per_Cluster()
    {
        // Arrange
        var clusters = new List<List<int>>
        {
            new() { 1, 10 },
            new() { 2, 20 },
            new() { 3, 30 }
        };
        const int numberCount = 3; // equals the number of clusters

        // Act
        var selectedNumbers = ClusteringAnalysisPredictionStrategyHelpers.SelectNumbersFromClusters(clusters, numberCount);

        // Assert
        selectedNumbers.Should().HaveCount(numberCount);
        selectedNumbers[0].Should().BeOneOf(clusters[0]);
        selectedNumbers[1].Should().BeOneOf(clusters[1]);
        selectedNumbers[2].Should().BeOneOf(clusters[2]);
    }

    [Test]
    public void Given_Clusters_With_Empty_And_NonEmpty_Lists_When_SelectNumbersFromClusters_Method_Is_Invoked_Then_Only_NonEmpty_Clusters_Contribute_Numbers()
    {
        // Arrange
        var clusters = new List<List<int>>
        {
            new(),                // empty cluster
            new() { 1, 2 },           // non-empty cluster
            new(),                // empty cluster
            new() { 3 }             // non-empty cluster
        };
        const int numberCount = 3; // request more than the available non-empty clusters

        // Act
        var selectedNumbers = ClusteringAnalysisPredictionStrategyHelpers.SelectNumbersFromClusters(clusters, numberCount);

        // Assert
        // Only clusters[1] and clusters[3] have numbers, so we expect two numbers.
        selectedNumbers.Should().HaveCount(2);
        selectedNumbers[0].Should().BeOneOf(clusters[1]);
        selectedNumbers[1].Should().BeOneOf(clusters[3]);
    }

    [Test]
    public void Given_A_Number_Count_Zero_When_SelectNumbersFromClusters_Method_Is_Invoked_Then_Return_Empty_List()
    {
        // Arrange
        var clusters = new List<List<int>>
        {
            new() { 1, 2, 3 },
            new() { 4, 5, 6 }
        };
        const int numberCount = 0;

        // Act
        var selectedNumbers = ClusteringAnalysisPredictionStrategyHelpers.SelectNumbersFromClusters(clusters, numberCount);

        // Assert
        selectedNumbers.Should().BeEmpty();
    }
}