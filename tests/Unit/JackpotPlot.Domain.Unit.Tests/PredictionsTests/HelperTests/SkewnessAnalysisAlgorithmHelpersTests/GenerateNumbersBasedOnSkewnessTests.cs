using FluentAssertions;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.SkewnessAnalysisAlgorithmHelpersTests;

[TestFixture]
public class GenerateNumbersBasedOnSkewnessTests
{
    [Test]
    public void Given_Count_When_GenerateNumbersBasedOnSkewness_Is_Invoked_Should_Return_Requested_Count()
    {
        // Arrange
        var rng = new Random(42);

        // Act
        var result = SkewnessAnalysisAlgorithmHelpers.GenerateNumbersBasedOnSkewness(
            maxRange: 40, count: 7, skewness: 0.0, rng: rng);

        // Assert
        result.Length.Should().Be(7);
    }

    [Test]
    public void Given_Positive_Skew_When_GenerateNumbersBasedOnSkewness_Is_Invoked_Should_Bias_Low_Half()
    {
        // Arrange
        var rng = new Random(7);

        // Act
        var result = SkewnessAnalysisAlgorithmHelpers.GenerateNumbersBasedOnSkewness(
            maxRange: 40, count: 12, skewness: +0.5, rng: rng);

        // Assert
        (result.Count(n => n <= 20) > result.Count(n => n > 20)).Should().BeTrue();
    }

    [Test]
    public void Given_Negative_Skew_When_GenerateNumbersBasedOnSkewness_Is_Invoked_Should_Bias_High_Half()
    {
        // Arrange
        var rng = new Random(11);

        // Act
        var result = SkewnessAnalysisAlgorithmHelpers.GenerateNumbersBasedOnSkewness(
            maxRange: 40, count: 12, skewness: -0.5, rng: rng);

        // Assert
        (result.Count(n => n > 20) > result.Count(n => n <= 20)).Should().BeTrue();
    }

    [Test]
    public void Given_Request_For_Unique_Numbers_When_GenerateNumbersBasedOnSkewness_Is_Invoked_Should_Return_Distinct_Values()
    {
        // Arrange
        var rng = new Random(99);

        // Act
        var result = SkewnessAnalysisAlgorithmHelpers.GenerateNumbersBasedOnSkewness(
            maxRange: 30, count: 10, skewness: 0.2, rng: rng);

        // Assert
        result.Should().OnlyHaveUniqueItems();
    }
}