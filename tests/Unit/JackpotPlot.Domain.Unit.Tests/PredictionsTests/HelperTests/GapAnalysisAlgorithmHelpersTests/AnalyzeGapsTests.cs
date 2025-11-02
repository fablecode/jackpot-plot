using FluentAssertions;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.GapAnalysisAlgorithmHelpersTests;

[TestFixture]
public class AnalyzeGapsTests
{
    [Test]
    public void Given_No_Draws_When_AnalyzeGaps_Method_Is_Invoked_Should_Return_Empty()
    {
        // Arrange
        var draws = Array.Empty<HistoricalDraw>();

        // Act
        var result = GapAnalysisAlgorithmHelpers.AnalyzeGaps(draws);

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public void Given_Draws_When_AnalyzeGaps_Method_Is_Invoked_Should_Count_Positive_Differences()
    {
        // Arrange
        var draws = new[]
        {
            AlgorithmsTestHelperTests.Draw(6,1,3,10), // sorted -> 1,3,6,10 => gaps 2,3,4
            AlgorithmsTestHelperTests.Draw(5,9)       // gap 4
        };

        // Act
        var result = GapAnalysisAlgorithmHelpers.AnalyzeGaps(draws);

        // Assert
        result.Should().Contain(new KeyValuePair<int, int>(4, 2));
    }
}