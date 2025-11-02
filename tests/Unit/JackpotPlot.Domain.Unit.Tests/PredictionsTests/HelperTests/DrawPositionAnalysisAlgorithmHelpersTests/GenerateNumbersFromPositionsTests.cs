using FluentAssertions;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.DrawPositionAnalysisAlgorithmHelpersTests;

[TestFixture]
public class GenerateNumbersFromPositionsTests
{
    [Test]
    public void Given_Distinct_Top_Picks_Per_Position_When_GenerateNumbersFromPositions_Method_Is_Invoked_Should_Return_One_Per_Position()
    {
        // Arrange
        var rng = new Random(123);
        var posFreq = new Dictionary<int, Dictionary<int, int>>
        {
            [0] = new() { [1] = 5, [2] = 1, [3] = 0 },
            [1] = new() { [2] = 7, [1] = 1, [3] = 0 },
            [2] = new() { [3] = 9, [1] = 1, [2] = 0 }
        };

        // Act
        var result = DrawPositionAnalysisAlgorithmHelpers.GenerateNumbersFromPositions(posFreq, rng).ToList();

        // Assert
        result.Count.Should().Be(3);
    }

    [Test]
    public void Given_Same_Number_Tops_Multiple_Positions_When_GenerateNumbersFromPositions_Method_Is_Invoked_Should_Return_Distinct_Numbers()
    {
        // Arrange
        var rng = new Random(456);
        var posFreq = new Dictionary<int, Dictionary<int, int>>
        {
            [0] = new() { [7] = 10, [1] = 1 },
            [1] = new() { [7] = 10, [2] = 9 }, // 7 would be duplicate; helper skips adding duplicates
            [2] = new() { [7] = 10, [3] = 8 }
        };

        // Act
        var result = DrawPositionAnalysisAlgorithmHelpers.GenerateNumbersFromPositions(posFreq, rng).ToList();

        // Assert
        result.Distinct().Count().Should().Be(result.Count);
    }
}