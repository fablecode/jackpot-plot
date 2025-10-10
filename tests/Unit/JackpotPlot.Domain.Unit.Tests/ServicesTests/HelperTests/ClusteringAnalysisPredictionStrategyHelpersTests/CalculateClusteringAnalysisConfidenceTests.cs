using System.Collections.Immutable;
using FluentAssertions;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.ServicesTests.HelperTests.ClusteringAnalysisPredictionStrategyHelpersTests;

[TestFixture]
public class CalculateClusteringAnalysisConfidenceTests
{
    [TestCase(new[] { 1, 2 }, new[] { 1, 2 }, 1.0, TestName = "All predictions match → confidence = 1")]
    [TestCase(new[] { 1, 2 }, new[] { 3, 4 }, 0.0, TestName = "No predictions match → confidence = 0")]
    [TestCase(new[] { 10, 20, 30, 40 }, new[] { 10, 99, 30, 100 }, 0.5, TestName = "Half predictions match → confidence = 0.5")]
    public void Given_Clusters_And_Predictions_When_CalculateConfidence_Is_Invoked_Should_Return_Expected(int[] clusterValues, int[] predictions, double expected)
    {
        // Arrange
        var clusters = new List<List<int>> { new List<int>(clusterValues) };
        var predictedNumbers = new List<int>(predictions);

        // Act
        var confidence = ClusteringAnalysisPredictionStrategyHelpers.CalculateClusteringAnalysisConfidence(clusters, predictedNumbers.ToImmutableArray());

        // Assert
        confidence.Should().Be(expected);
    }

    [Test]
    public void Given_Empty_Predictions_When_CalculateConfidence_Method_Is_Invoked_Should_Return_NaN()
    {
        // Arrange
        var clusters = new List<List<int>> { new() { 1, 2 } };
        var predictions = new List<int>();

        // Act
        var confidence = ClusteringAnalysisPredictionStrategyHelpers.CalculateClusteringAnalysisConfidence(clusters, predictions.ToImmutableArray());

        // Assert
        confidence.Should().Be(double.NaN);
    }

    [Test]
    public void Given_Duplicate_Predictions_When_CalculateConfidence_Method_Is_Invoked_Then_Denominator_Includes_All()
    {
        // Arrange
        var clusters = new List<List<int>> { new() { 1, 2 } };
        var predictions = new List<int> { 1, 1, 3 }; // 2 matches out of 3

        // Act
        var confidence = ClusteringAnalysisPredictionStrategyHelpers.CalculateClusteringAnalysisConfidence(clusters, predictions.ToImmutableArray());

        // Assert
        confidence.Should().BeApproximately(2.0 / 3.0, 1e-9);
    }
}