using FluentAssertions;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.NumberSumAlgorithmHelpersTests;

[TestFixture]
public class GenerateNumbersWithTargetSumTests
{
    [Test]
    public void Given_TargetSum_When_GenerateNumbersWithTargetSum_Method_Is_Invoked_Should_Return_Requested_Count()
    {
        // Arrange
        var rng = new Random(1);

        // Act
        var numbers = NumberSumAlgorithmHelpers.GenerateNumbersWithTargetSum(maxRange: 50, count: 5, targetSum: 100.0, rng);

        // Assert
        numbers.Count.Should().Be(5);
    }

    [Test]
    public void Given_Range_When_GenerateNumbersWithTargetSum_Method_Is_Invoked_Should_Return_All_In_Range()
    {
        // Arrange
        var rng = new Random(2);

        // Act
        var numbers = NumberSumAlgorithmHelpers.GenerateNumbersWithTargetSum(maxRange: 20, count: 6, targetSum: 60.0, rng);

        // Assert
        numbers.All(n => n is >= 1 and <= 20).Should().BeTrue();
    }

    [Test]
    public void Given_TargetSum_When_GenerateNumbersWithTargetSum_Method_Is_Invoked_Should_Return_Distinct_Numbers()
    {
        // Arrange
        var rng = new Random(3);

        // Act
        var numbers = NumberSumAlgorithmHelpers.GenerateNumbersWithTargetSum(maxRange: 30, count: 7, targetSum: 105.0, rng);

        // Assert
        numbers.Distinct().Count().Should().Be(numbers.Count);
    }

    [Test]
    public void Given_TargetSum_When_GenerateNumbersWithTargetSum_Method_Is_Invoked_Should_Approximate_Target_Sum()
    {
        // Arrange
        var rng = new Random(4);
        const double target = 120.0;

        // Act
        var numbers = NumberSumAlgorithmHelpers.GenerateNumbersWithTargetSum(maxRange: 50, count: 6, targetSum: target, rng);

        // Assert
        Math.Abs(numbers.Sum() - target).Should().BeLessThanOrEqualTo(15.0);
    }
}