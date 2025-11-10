using FluentAssertions;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.QuadrantAnalysisAlgorithmHelpersTests;

[TestFixture]
public class AnalyzeQuadrantFrequenciesTests
{
    [Test]
    public void Given_Draws_When_AnalyzeQuadrantFrequencies_Is_Called_Should_Count_Numbers_In_Each_Quadrant()
    {
        // Arrange
        var quads = new List<(int start, int end)> { (1, 5), (6, 10) };
        var draws = new[] { DivideIntoQuadrantsTests.Draw(1, 2, 6, 10) };

        // Act
        var freq = QuadrantAnalysisAlgorithmHelpers.AnalyzeQuadrantFrequencies(draws, quads);

        // Assert
        freq[(1, 5)].Should().Be(2);
    }
}