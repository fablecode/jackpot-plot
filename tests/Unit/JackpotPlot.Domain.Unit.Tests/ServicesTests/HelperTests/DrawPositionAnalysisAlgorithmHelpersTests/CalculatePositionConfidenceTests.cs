using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;
using System.Collections.Immutable;
using FluentAssertions;

namespace JackpotPlot.Domain.Unit.Tests.ServicesTests.HelperTests.DrawPositionAnalysisAlgorithmHelpersTests;

[TestFixture]
public class CalculatePositionConfidenceTests
{
    [Test]
    public void Given_No_Draws_When_CalculatePositionConfidence_Method_Is_Invoked_Should_Return_Zero()
    {
        // Arrange
        var draws = Array.Empty<HistoricalDraw>();
        var predicted = ImmutableArray.Create(1, 2, 3);

        // Act
        var confidence = DrawPositionAnalysisAlgorithmHelpers.CalculatePositionConfidence(draws, predicted);

        // Assert
        confidence.Should().Be(0.0);
    }

    [Test]
    public void Given_Empty_Predicted_When_CalculatePositionConfidence_Method_Is_Invoked_Should_Return_Zero()
    {
        // Arrange
        var draws = new[] { AlgorithmsTestHelperTests.Draw(1, 2, 3) };
        var predicted = ImmutableArray<int>.Empty;

        // Act
        var confidence = DrawPositionAnalysisAlgorithmHelpers.CalculatePositionConfidence(draws, predicted);

        // Assert
        confidence.Should().Be(0.0);
    }

    [Test]
    public void Given_Mixed_Matches_When_CalculatePositionConfidence_Method_Is_Invoked_Should_Return_Expected_Ratio()
    {
        // Arrange
        // d1 vs predicted [1,99,3] -> matches at pos0 and pos2 (2/3)
        // d2 vs predicted [1,99,3] -> match at pos0 only (1/3)
        // total matches = 3, total positions = 6 -> 0.5
        var draws = new[] { AlgorithmsTestHelperTests.Draw(1, 2, 3), AlgorithmsTestHelperTests.Draw(1, 5, 6) };
        var predicted = ImmutableArray.Create(1, 99, 3);

        // Act
        var confidence = DrawPositionAnalysisAlgorithmHelpers.CalculatePositionConfidence(draws, predicted);

        // Assert
        confidence.Should().BeApproximately(0.5, 1e-9);
    }
}