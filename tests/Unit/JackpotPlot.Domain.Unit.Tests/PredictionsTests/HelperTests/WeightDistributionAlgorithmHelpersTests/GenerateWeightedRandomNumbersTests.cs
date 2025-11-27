using FluentAssertions;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.WeightDistributionAlgorithmHelpersTests;

[TestFixture]
public class GenerateWeightedRandomNumbersTests
{
    [Test]
    public void Given_Non_Positive_Count_When_GenerateWeightedRandomNumbers_Is_Invoked_Should_Return_Empty()
    {
        // Arrange
        var weights = new Dictionary<int, double> { { 1, 1.0 } };
        var rng = new Random(1);

        // Act
        var result = WeightDistributionAlgorithmHelpers.GenerateWeightedRandomNumbers(weights, 0, rng);

        // Assert
        result.IsEmpty.Should().BeTrue();
    }

    [Test]
    public void Given_Positive_Weights_When_GenerateWeightedRandomNumbers_Is_Invoked_Should_Respect_Weights()
    {
        // Arrange
        // Only number 1 has positive weight: it must be picked first
        var weights = new Dictionary<int, double>
        {
            { 1, 1.0 },
            { 2, 0.0 },
            { 3, 0.0 }
        };
        var rng = new Random(2);

        // Act
        var result = WeightDistributionAlgorithmHelpers.GenerateWeightedRandomNumbers(weights, 1, rng);

        // Assert
        result.Single().Should().Be(1);
    }

    [Test]
    public void Given_Weights_When_GenerateWeightedRandomNumbers_Is_Invoked_Should_Not_Return_Duplicates()
    {
        // Arrange
        var weights = new Dictionary<int, double>
        {
            { 1, 1.0 },
            { 2, 1.0 },
            { 3, 1.0 }
        };
        var rng = new Random(3);

        // Act
        var result = WeightDistributionAlgorithmHelpers.GenerateWeightedRandomNumbers(weights, 3, rng);

        // Assert
        (result.Length == 3 && result.Distinct().Count() == 3).Should().BeTrue();
    }

    [Test]
    public void Given_All_Zero_Weights_When_GenerateWeightedRandomNumbers_Is_Invoked_Should_Fallback_To_Uniform()
    {
        // Arrange
        var weights = new Dictionary<int, double>
        {
            { 1, 0.0 },
            { 2, 0.0 },
            { 3, 0.0 }
        };
        var rng = new Random(4);

        // Act
        var result = WeightDistributionAlgorithmHelpers.GenerateWeightedRandomNumbers(weights, 2, rng);

        // Assert
        (result.Length == 2 && result.All(n => weights.ContainsKey(n))).Should().BeTrue();
    }
}