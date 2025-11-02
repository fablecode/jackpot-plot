using FluentAssertions;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.DrawPositionAnalysisAlgorithmHelpersTests;

[TestFixture]
public class AnalyzeDrawPositionsTests
{
    [Test]
    public void Given_No_Draws_When_AnalyzeDrawPositions_Method_Is_Invoked_Should_Create_Tables_For_All_Positions()
    {
        // Arrange
        var draws = Array.Empty<HistoricalDraw>();

        // Act
        var map = DrawPositionAnalysisAlgorithmHelpers.AnalyzeDrawPositions(draws, mainNumbersCount: 3, numberRange: 10);

        // Assert
        map.Count.Should().Be(3);
    }

    [Test]
    public void Given_Draws_When_AnalyzeDrawPositions_Method_Is_Invoked_Should_Increment_Frequency_For_Number_At_Position()
    {
        // Arrange
        var draws = new[]
        {
            AlgorithmsTestHelperTests.Draw(5, 6, 7),
            AlgorithmsTestHelperTests.Draw(5, 8) // position 0 has number 5 again
        };

        // Act
        var map = DrawPositionAnalysisAlgorithmHelpers.AnalyzeDrawPositions(draws, mainNumbersCount: 3, numberRange: 10);

        // Assert
        map[0][5].Should().Be(2);
    }

    [Test]
    public void Given_NumberRange_When_AnalyzeDrawPositions_Method_Is_Invoked_Should_Initialize_All_Number_Keys_For_A_Position()
    {
        // Arrange
        var draws = new[] { AlgorithmsTestHelperTests.Draw(1, 2, 3) };

        // Act
        var map = DrawPositionAnalysisAlgorithmHelpers.AnalyzeDrawPositions(draws, mainNumbersCount: 2, numberRange: 7);

        // Assert
        map[1].Count.Should().Be(7);
    }
}