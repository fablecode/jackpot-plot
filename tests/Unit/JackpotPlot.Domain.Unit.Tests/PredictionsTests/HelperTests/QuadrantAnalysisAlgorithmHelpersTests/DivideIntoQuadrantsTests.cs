using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;
using FluentAssertions;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.QuadrantAnalysisAlgorithmHelpersTests;

[TestFixture]
public class DivideIntoQuadrantsTests
{
    public static HistoricalDraw Draw(params int[] main) =>
           new HistoricalDraw(
               DrawId: 1,
               LotteryId: 1,
               DrawDate: DateTime.UtcNow,
               WinningNumbers: main.ToList(),
               BonusNumbers: new List<int>(),
               CreatedAt: DateTime.UtcNow);

    [Test]
    public void Given_Range_And_QuadrantCount_When_DivideIntoQuadrants_Method_Is_Invoked_Should_Cover_Full_Range()
    {
        // Arrange
        const int maxRange = 17;
        const int quadrants = 4;

        // Act
        var result = QuadrantAnalysisAlgorithmHelpers.DivideIntoQuadrants(maxRange, quadrants);

        // Assert
        result.Last().end.Should().Be(maxRange);
    }

    [Test]
    public void Given_Non_Divisible_Range_When_DivideIntoQuadrants_Method_Is_Invoked_Should_Use_Integer_Block_Size_For_Middle_Quadrants()
    {
        // Arrange
        const int maxRange = 10;
        const int quadrants = 3;

        // Act
        var result = QuadrantAnalysisAlgorithmHelpers.DivideIntoQuadrants(maxRange, quadrants);

        // Assert
        (result[0].start == 1 && result[0].end == 3).Should().BeTrue();
    }
}