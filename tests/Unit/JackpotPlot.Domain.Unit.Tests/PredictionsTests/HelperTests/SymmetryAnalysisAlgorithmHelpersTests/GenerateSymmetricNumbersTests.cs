using FluentAssertions;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.SymmetryAnalysisAlgorithmHelpersTests;

[TestFixture]
public class GenerateSymmetricNumbersTests
{
    [Test]
    public void Given_Non_Positive_Count_When_GenerateSymmetricNumbers_Is_Invoked_Should_Return_Empty()
    {
        // Arrange
        var rng = new Random(1);

        // Act
        var result = SymmetryAnalysisAlgorithmHelpers.GenerateSymmetricNumbers((1, 1), 0, 50, rng);

        // Assert
        result.IsEmpty.Should().BeTrue();
    }

    [Test]
    public void Given_Ratios_When_GenerateSymmetricNumbers_Is_Invoked_Should_Return_Requested_Count()
    {
        // Arrange
        var rng = new Random(2);

        // Act
        var result = SymmetryAnalysisAlgorithmHelpers.GenerateSymmetricNumbers((1, 1), 10, 50, rng);

        // Assert
        result.Length.Should().Be(10);
    }

    [Test]
    public void Given_Strong_High_Ratio_When_GenerateSymmetricNumbers_Is_Invoked_Should_Favor_High_Values()
    {
        // Arrange
        var rng = new Random(3);
        var metrics = (highLowRatio: 10.0, oddEvenRatio: 1.0);

        // Act
        var result = SymmetryAnalysisAlgorithmHelpers.GenerateSymmetricNumbers(metrics, 12, 40, rng);
        var mid = 40 / 2;
        var highCount = result.Count(n => n > mid);

        // Assert
        highCount.Should().BeGreaterThan(result.Length / 2);
    }

    [Test]
    public void Given_Strong_Odd_Ratio_When_GenerateSymmetricNumbers_Is_Invoked_Should_Favor_Odd_Numbers()
    {
        // Arrange
        var rng = new Random(4);
        var metrics = (highLowRatio: 1.0, oddEvenRatio: 10.0);

        // Act
        var result = SymmetryAnalysisAlgorithmHelpers.GenerateSymmetricNumbers(metrics, 12, 40, rng);
        var oddCount = result.Count(n => (n & 1) == 1);

        // Assert
        oddCount.Should().BeGreaterThan(result.Length / 2);
    }

    [Test]
    public void Given_Small_Range_When_GenerateSymmetricNumbers_Is_Invoked_Should_Not_Exceed_NumberRange()
    {
        // Arrange
        var rng = new Random(5);
        var metrics = (highLowRatio: 0.5, oddEvenRatio: 0.5);

        // Act
        var result = SymmetryAnalysisAlgorithmHelpers.GenerateSymmetricNumbers(metrics, 8, 6, rng);

        // Assert
        result.Length.Should().Be(6); // cannot exceed numberRange (1..6)
    }
}