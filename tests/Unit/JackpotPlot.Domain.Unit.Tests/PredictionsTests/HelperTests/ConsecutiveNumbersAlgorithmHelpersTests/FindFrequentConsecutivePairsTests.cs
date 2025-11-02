using FluentAssertions;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.ConsecutiveNumbersAlgorithmHelpersTests;

[TestFixture]
public class FindFrequentConsecutivePairsTests
{
    [Test]
    public void Given_No_Draws_When_FindFrequentConsecutivePairs_Method_Is_Invoked_Should_Return_Empty()
    {
        // Arrange
        var draws = Array.Empty<HistoricalDraw>();

        // Act
        var result = ConsecutiveNumbersAlgorithmHelpers.FindFrequentConsecutivePairs(draws);

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public void Given_Draws_With_Consecutive_Pairs_When_FindFrequentConsecutivePairs_Method_Is_Invoked_Should_Aggregate_Counts()
    {
        // Arrange
        var draws = new[]
        {
            AlgorithmsTestHelperTests.Draw(1,2,4,5),   // (1,2) and (4,5)
            AlgorithmsTestHelperTests.Draw(1,2,3),     // (1,2) and (2,3)
            AlgorithmsTestHelperTests.Draw(10,11)      // (10,11)
        };

        // Act
        var result = ConsecutiveNumbersAlgorithmHelpers.FindFrequentConsecutivePairs(draws);

        // Assert
        result[(1, 2)].Should().Be(2);
    }
}