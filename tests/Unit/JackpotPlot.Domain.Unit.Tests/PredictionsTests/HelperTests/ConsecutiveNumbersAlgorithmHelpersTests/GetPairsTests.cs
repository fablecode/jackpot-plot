using FluentAssertions;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.ConsecutiveNumbersAlgorithmHelpersTests;

[TestFixture]
public class GetPairsTests
{
    [Test]
    public void Given_Unordered_List_When_GetPairs_Method_Is_Invoked_Result_Should_Only_Consider_Adjacent_Consecutive()
    {
        // Arrange
        // Only adjacent consecutive numbers count: (1,2) and (2,3) are adjacent here, (3,4) isn't (gap 5 in between)
        var numbers = new List<int> { 7, 1, 2, 3, 5, 4 };

        // Act
        var result = ConsecutiveNumbersAlgorithmHelpers.GetPairs(numbers);

        // Assert
        result.Should().BeEquivalentTo(new List<(int, int)> { (1, 2), (2, 3) });
    }

    [Test]
    public void Given_Straight_Run_When_GetPairs_Method_Is_Invoked_Should_Return_All_Adjacent_Pairs()
    {
        // Arrange
        var numbers = new List<int> { 10, 11, 12, 13 };

        // Act
        var result = ConsecutiveNumbersAlgorithmHelpers.GetPairs(numbers);

        // Assert
        result.Should().BeEquivalentTo(new List<(int, int)> { (10, 11), (11, 12), (12, 13) });
    }
}