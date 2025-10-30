using FluentAssertions;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.ServicesTests.HelperTests.ConsecutiveNumbersAlgorithmHelpersTests;

[TestFixture]
public class SelectConsecutiveNumbersTests
{
    [Test]
    public void Given_Empty_Pairs_When_SelectConsecutiveNumbers_Method_Is_Invoked_Should_Return_Empty_List()
    {
        // Arrange
        var pairs = new Dictionary<(int, int), int>();

        // Act
        var result = ConsecutiveNumbersAlgorithmHelpers.SelectConsecutiveNumbers(pairs, maxCount: 5);

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public void Given_Pairs_When_SelectConsecutiveNumbers_Method_Is_Invoked_Result_Should_Not_Exceed_MaxCount()
    {
        // Arrange
        var pairs = new Dictionary<(int, int), int>
        {
            [(1, 2)] = 7,
            [(2, 3)] = 6,
            [(50, 51)] = 5,
            [(10, 11)] = 4
        };

        // Act
        var result = ConsecutiveNumbersAlgorithmHelpers.SelectConsecutiveNumbers(pairs, maxCount: 3);

        // Assert
        result.Count.Should().Be(3);
    }

    [Test]
    public void Given_Pairs_When_SelectConsecutiveNumbers_Method_Is_Invoked_Result_Should_Contain_Elements_From_Most_Frequent_Pair()
    {
        // Arrange
        var pairs = new Dictionary<(int, int), int>
        {
            [(8, 9)] = 2,
            [(1, 2)] = 10,
            [(10, 11)] = 1
        };

        // Act
        var result = ConsecutiveNumbersAlgorithmHelpers.SelectConsecutiveNumbers(pairs, maxCount: 2);

        // Assert
        result.Should().Contain([1, 2]);
    }
}