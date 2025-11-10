using System.Collections.Immutable;
using FluentAssertions;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.LastAppearanceAlgorithmHelpersTests;

[TestFixture]
public class DistributionConfidenceTests
{
    [Test]
    public void Given_Empty_Predicted_When_DistributionConfidence_Method_Is_Invoked_Should_Return_Zero()
    {
        // Arrange
        var draws = new[] { AlgorithmsTestHelperTests.Draw(1, 2, 6, 7) };
        var predicted = ImmutableArray<int>.Empty;

        // Act
        var confidence = HighLowNumberSplitAlgorithmHelpers.DistributionConfidence(draws, predicted, numberRange: 10);

        // Assert
        confidence.Should().Be(0.0);
    }

    [Test]
    public void Given_Predicted_Matches_Historical_Distribution_When_DistributionConfidence_Method_Is_Invoked_Should_Return_One()
    {
        // Arrange
        var draws = new[] { AlgorithmsTestHelperTests.Draw(1, 2, 6, 7) };
        var predicted = ImmutableArray.Create(1, 2, 6, 7); // also 50/50

        // Act
        var confidence = HighLowNumberSplitAlgorithmHelpers.DistributionConfidence(draws, predicted, numberRange: 10);

        // Assert
        confidence.Should().Be(1.0);
    }

    [Test]
    public void Given_Opposite_Distributions_When_DistributionConfidence_Method_Is_Invoked_Should_Return_Half()
    {
        // Arrange
        var draws = new[]
        {
            AlgorithmsTestHelperTests.Draw(1,2,3,4),   // lows=4
            AlgorithmsTestHelperTests.Draw(1,2,6,7)    // lows=2, highs=2
        };
        var predicted = ImmutableArray.Create(6, 7, 8, 2);

        // Act
        var confidence = HighLowNumberSplitAlgorithmHelpers.DistributionConfidence(draws, predicted, numberRange: 10);

        // Assert
        confidence.Should().BeApproximately(0.5, 1e-9);
    }
}