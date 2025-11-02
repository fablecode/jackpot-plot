using FluentAssertions;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.CyclicPatternsAlgorithmHelpersTests;

[TestFixture]
public class GenerateNumbersFromCyclesTests
{
    [Test]
    public void Given_Empty_Cycles_When_GenerateNumbersFromCycles_Method_Is_Invoked_Should_Return_Empty_List()
    {
        // Arrange
        var cycles = new Dictionary<int, List<int>>();
        var rng = new Random(123);

        // Act
        var result = CyclicPatternsAlgorithmHelpers.GenerateNumbersFromCycles(cycles, count: 5, rng);

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public void Given_Cycles_With_Different_Averages_When_GenerateNumbersFromCycles_Method_Is_Invoked_Should_Include_Number_With_Shortest_Average()
    {
        // Arrange
        // avg(1) = 1, avg(3) = 3, avg(5) = 2 → 1 is shortest
        var cycles = new Dictionary<int, List<int>>
        {
            [1] = [1, 1],
            [3] = [3],
            [5] = [2, 2],
            [8] = [] // ignored (no gaps)
        };
        var rng = new Random(321);

        // Act
        var result = CyclicPatternsAlgorithmHelpers.GenerateNumbersFromCycles(cycles, count: 2, rng);

        // Assert
        result.Should().Contain(1);
    }
}