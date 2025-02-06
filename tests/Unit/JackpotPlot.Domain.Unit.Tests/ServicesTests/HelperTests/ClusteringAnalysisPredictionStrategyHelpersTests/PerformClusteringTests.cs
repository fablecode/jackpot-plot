using FluentAssertions;
using JackpotPlot.Domain.Services.PredictionStrategies.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.ServicesTests.HelperTests.ClusteringAnalysisPredictionStrategyHelpersTests;

[TestFixture]
public class PerformClusteringTests
{
    [Test]
    public void Given_A_Valid_CoOccurrenceMatrix_And_Cluster_Count_Of_3_When_PerformClustering_Method_Is_Invoked_Then_All_Numbers_Are_Assigned_To_One_Cluster()
    {
        // Arrange
        // Create a co-occurrence matrix sized to include numbers 1 through 5 (matrix dimensions: 6x6).
        const int matrixSize = 6; // 0 to 5
        var coOccurrenceMatrix = new int[matrixSize, matrixSize];
        const int clusterCount = 3;

        // Act
        var clusters = ClusteringAnalysisPredictionStrategyHelpers.PerformClustering(coOccurrenceMatrix, clusterCount);

        // Assert
        clusters.Count.Should().Be(clusterCount);

        // Flatten clusters and verify that all numbers from 1 to 5 are present.
        var allAssignedNumbers = clusters.SelectMany(c => c).ToList();
        allAssignedNumbers.Count.Should().Be(matrixSize - 1);
        allAssignedNumbers.Should().BeEquivalentTo(Enumerable.Range(1, matrixSize - 1));

        // Verify that each number appears only once.
        allAssignedNumbers.GroupBy(n => n).All(g => g.Count() == 1).Should().BeTrue();
    }

    [Test]
    public void Given_A_Valid_CoOccurrenceMatrix_And_Cluster_Count_Of_1_When_PerformClustering_Method_Is_Invoked_Then_All_Numbers_Are_In_A_Single_Cluster()
    {
        // Arrange
        const int matrixSize = 6; // Numbers 1 to 5
        var coOccurrenceMatrix = new int[matrixSize, matrixSize];
        const int clusterCount = 1;

        // Act
        var clusters = ClusteringAnalysisPredictionStrategyHelpers.PerformClustering(coOccurrenceMatrix, clusterCount);

        // Assert
        clusters.Count.Should().Be(1);
        clusters[0].Should().BeEquivalentTo(Enumerable.Range(1, matrixSize - 1));
    }

    [Test]
    public void Given_A_CoOccurrenceMatrix_With_No_Numbers_When_PerformClustering_Method_Is_Invoked_Then_Clusters_Are_Empty()
    {
        // Arrange
        // If the matrix has a size of 1, the loop (starting at 1) never executes.
        const int matrixSize = 1; // No valid numbers (since loop runs for number from 1 to 0)
        var coOccurrenceMatrix = new int[matrixSize, matrixSize];
        const int clusterCount = 3;

        // Act
        var clusters = ClusteringAnalysisPredictionStrategyHelpers.PerformClustering(coOccurrenceMatrix, clusterCount);

        // Assert
        clusters.Count.Should().Be(clusterCount);
        foreach (var cluster in clusters)
        {
            cluster.Should().BeEmpty();
        }
    }

    [Test]
    public void Given_A_Zero_Cluster_Count_When_PerformClustering_Method_Is_Invoked_Then_ArgumentOutOfRangeException_Is_Thrown()
    {
        // Arrange
        const int matrixSize = 6;
        var coOccurrenceMatrix = new int[matrixSize, matrixSize];
        const int clusterCount = 0;

        // Act
        Action act = () => ClusteringAnalysisPredictionStrategyHelpers.PerformClustering(coOccurrenceMatrix, clusterCount);

        // Assert: random.Next(0, 0) should throw an ArgumentOutOfRangeException.
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}