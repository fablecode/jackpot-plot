using FluentAssertions;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.StandardDeviationAlgorithmHelpersTests;

[TestFixture]
public class GenerateNumbersWithTargetStdDevTests
{
    [Test]
    public void Given_Non_Positive_Count_When_GenerateNumbersWithTargetStdDev_Is_Invoked_Should_Return_Empty()
    {
        // Arrange
        var rng = new Random(1);

        // Act
        var result = StandardDeviationAlgorithmHelpers.GenerateNumbersWithTargetStdDev(40, 0, 5.0, rng);

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public void Given_TargetStdDev_When_GenerateNumbersWithTargetStdDev_Is_Invoked_Should_Approximate_Target()
    {
        // Arrange
        var rng = new Random(42);
        var target = 6.0;

        // Act
        var result = StandardDeviationAlgorithmHelpers.GenerateNumbersWithTargetStdDev(50, 12, target, rng);

        // Assert
        Math.Abs(StdDev(result) - target).Should().BeLessThan(1.5);
    }

    [Test]
    public void Given_Request_For_Unique_When_GenerateNumbersWithTargetStdDev_Is_Invoked_Should_Return_Unique_Values()
    {
        // Arrange
        var rng = new Random(7);

        // Act
        var result = StandardDeviationAlgorithmHelpers.GenerateNumbersWithTargetStdDev(30, 10, 4.0, rng);

        // Assert
        result.Distinct().Count().Should().Be(result.Count);
    }

    private static double StdDev(IReadOnlyCollection<int> values)
    {
        if (values.Count == 0) return 0;
        var mean = values.Average();
        var variance = values.Sum(v => Math.Pow(v - mean, 2)) / values.Count;
        return Math.Sqrt(variance);
    }

}