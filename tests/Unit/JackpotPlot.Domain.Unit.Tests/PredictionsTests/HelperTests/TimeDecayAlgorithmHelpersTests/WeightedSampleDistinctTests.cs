using FluentAssertions;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.TimeDecayAlgorithmHelpersTests;

[TestFixture]
public class WeightedSampleDistinctTests
{
    [Test]
    public void Given_Non_Positive_Take_When_WeightedSampleDistinct_Is_Invoked_Should_Return_Empty_Sequence()
    {
        // Arrange
        var weights = new Dictionary<int, double> { [1] = 1.0, [2] = 2.0 };
        var rng = new Random(1);

        // Act
        var result = TimeDecayAlgorithmHelpers.WeightedSampleDistinct(weights, 0, rng).ToList();

        // Assert
        result.Count.Should().Be(0);
    }

    [Test]
    public void Given_Positive_Weights_When_WeightedSampleDistinct_Is_Invoked_Should_Return_Distinct_Keys()
    {
        // Arrange
        var weights = new Dictionary<int, double>
        {
            [1] = 1.0,
            [2] = 2.0,
            [3] = 3.0
        };
        var rng = new Random(2);

        // Act
        var result = TimeDecayAlgorithmHelpers.WeightedSampleDistinct(weights, 3, rng).ToList();

        // Assert
        (result.Count == 3 && result.Distinct().Count() == 3 && result.All(weights.ContainsKey)).Should().BeTrue();
    }

    [Test]
    public void Given_Zero_Total_Weights_When_WeightedSampleDistinct_Is_Invoked_Should_Fallback_To_Uniform()
    {
        // Arrange
        var weights = new Dictionary<int, double>
        {
            [1] = 0.0,
            [2] = 0.0,
            [3] = 0.0
        };
        var rng = new Random(3);

        // Act
        var result = TimeDecayAlgorithmHelpers.WeightedSampleDistinct(weights, 2, rng).ToList();

        // Assert
        (result.Count == 2 && result.All(weights.ContainsKey)).Should().BeTrue();
    }
}