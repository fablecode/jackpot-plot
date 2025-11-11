using FluentAssertions;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.ReducedNumberPoolAlgorithmHelpersTests;

[TestFixture]
public class GenerateFromReducedPoolTests
{
    [Test]
    public void Given_Non_Positive_Take_When_GenerateFromReducedPool_Is_Invoked_Should_Return_Empty()
    {
        // Arrange
        var pool = new List<int> { 1, 2, 3 };
        var rng = new Random(1);

        // Act
        var result = ReducedNumberPoolAlgorithmHelpers.GenerateFromReducedPool(pool, numberRange: 10, take: 0, rng).ToList();

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public void Given_Sufficient_Pool_When_GenerateFromReducedPool_Is_Invoked_Should_Select_Only_From_Pool()
    {
        // Arrange
        var pool = new List<int> { 2, 4, 6, 8, 10 };
        var rng = new Random(2);

        // Act
        var result = ReducedNumberPoolAlgorithmHelpers.GenerateFromReducedPool(pool, numberRange: 10, take: 4, rng).ToList();

        // Assert
        result.All(n => pool.Contains(n)).Should().BeTrue();
    }

    [Test]
    public void Given_Insufficient_Pool_When_GenerateFromReducedPool_Is_Invoked_Should_Top_Up_From_Full_Range()
    {
        // Arrange
        var pool = new List<int> { 1 }; // need 3 → 1 from pool + 2 from full range
        var rng = new Random(3);

        // Act
        var result = ReducedNumberPoolAlgorithmHelpers.GenerateFromReducedPool(pool, numberRange: 5, take: 3, rng).ToList();

        // Assert
        result.Count.Should().Be(3);
    }

    [Test]
    public void Given_Range_When_GenerateFromReducedPool_Is_Invoked_Result_Should_Be_In_Range_And_Distinct()
    {
        // Arrange
        var pool = new List<int> { 1, 3, 5 };
        var rng = new Random(4);

        // Act
        var result = ReducedNumberPoolAlgorithmHelpers.GenerateFromReducedPool(pool, numberRange: 10, take: 5, rng).ToList();

        // Assert
        (result.All(n => n is >= 1 and <= 10) && result.Distinct().Count() == result.Count).Should().BeTrue();
    }
}