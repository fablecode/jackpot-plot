using FluentAssertions;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.QuadrantAnalysisAlgorithmHelpersTests;

[TestFixture]
public class CalculateQuadrantConfidenceTests
{
    [Test]
    public void Given_Identical_Distributions_When_CalculateQuadrantConfidence_Is_Called_Should_Return_One()
    {
        // Arrange
        var quads = new List<(int start, int end)> { (1, 5), (6, 10) };
        var draws = new[] { DivideIntoQuadrantsTests.Draw(1, 6) };      // 1 in each quadrant
        var predicted = new List<int> { 2, 9 }; // 1 in each quadrant

        // Act
        var confidence = QuadrantAnalysisAlgorithmHelpers.CalculateQuadrantConfidence(draws, predicted, quads);

        // Assert
        confidence.Should().Be(1.0);
    }

    [Test]
    public void Given_Opposite_Distributions_When_CalculateQuadrantConfidence_Is_Called_Should_Return_Point_Two()
    {
        // Arrange
        var quads = new List<(int start, int end)> { (1, 5), (6, 10) };
        var draws = new[] { DivideIntoQuadrantsTests.Draw(1, 1) };
        var predicted = new List<int> { 7, 8 };

        // Act
        var confidence = QuadrantAnalysisAlgorithmHelpers.CalculateQuadrantConfidence(draws, predicted, quads);

        // Assert
        confidence.Should().BeApproximately(0.2, 1e-9);
    }
}