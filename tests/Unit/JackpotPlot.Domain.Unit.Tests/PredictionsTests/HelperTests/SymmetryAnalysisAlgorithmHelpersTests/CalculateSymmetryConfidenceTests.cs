using System.Collections.Immutable;
using FluentAssertions;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.SymmetryAnalysisAlgorithmHelpersTests;

[TestFixture]
public class CalculateSymmetryConfidenceTests
{
    [Test]
    public void Given_Empty_Predicted_When_CalculateSymmetryConfidence_Is_Invoked_Should_Return_Zero()
    {
        // Arrange
        var predicted = ImmutableArray<int>.Empty;
        var metrics = (1.0, 1.0);

        // Act
        var result = SymmetryAnalysisAlgorithmHelpers.CalculateSymmetryConfidence(predicted, metrics, 40);

        // Assert
        result.Should().Be(0d);
    }

    [Test]
    public void Given_Predicted_Matching_Metrics_When_CalculateSymmetryConfidence_Is_Invoked_Should_Return_One()
    {
        // Arrange
        var metrics = (highLowRatio: 1.0, oddEvenRatio: 1.0);
        var predicted = ImmutableArray.Create(3, 4, 6, 7); // perfectly matches ratios
        var expected = 1.0 / (1 + 0 + 0);

        // Act
        var result = SymmetryAnalysisAlgorithmHelpers.CalculateSymmetryConfidence(predicted, metrics, 10);

        // Assert
        result.Should().Be(expected);
    }

    [Test]
    public void Given_Predicted_Different_From_Metrics_When_CalculateSymmetryConfidence_Is_Invoked_Should_Return_Less_Than_One()
    {
        // Arrange
        var metrics = (highLowRatio: 1.0, oddEvenRatio: 2.0);
        var predicted = ImmutableArray.Create(1, 3, 5, 7, 9, 11);

        // Act
        var result = SymmetryAnalysisAlgorithmHelpers.CalculateSymmetryConfidence(predicted, metrics, 12);

        // Assert
        result.Should().BeLessThan(1.0);
    }
}