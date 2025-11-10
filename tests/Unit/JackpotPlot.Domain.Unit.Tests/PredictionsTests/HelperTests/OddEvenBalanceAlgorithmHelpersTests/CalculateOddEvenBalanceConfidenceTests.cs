using FluentAssertions;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.OddEvenBalanceAlgorithmHelpersTests;

[TestFixture]
public class CalculateOddEvenBalanceConfidenceTests
{
    [Test]
    public void Given_Empty_Predicted_When_CalculateOddEvenBalanceConfidence_Method_Is_Invoked_Should_Return_Zero()
    {
        // Arrange
        var draws = new[] { AlgorithmsTestHelperTests.Draw(1, 2, 3, 4) };

        // Act
        var confidence = OddEvenBalanceAlgorithmHelpers.CalculateOddEvenBalanceConfidence(draws, []);

        // Assert
        confidence.Should().Be(0.0);
    }

    [Test]
    public void Given_Empty_History_When_CalculateOddEvenBalanceConfidence_Method_Is_Invoked_Should_Return_Zero()
    {
        // Arrange
        var predicted = new List<int> { 1, 2 };

        // Act
        var confidence = OddEvenBalanceAlgorithmHelpers.CalculateOddEvenBalanceConfidence(Array.Empty<HistoricalDraw>(), predicted);

        // Assert
        confidence.Should().Be(0.0);
    }

    [Test]
    public void Given_Matching_Distributions_When_CalculateOddEvenBalanceConfidence_Method_Is_Invoked_Should_Return_One()
    {
        // Arrange
        var predicted = new List<int> { 1, 2, 3, 4 };
        var draws = new[]
        {
            AlgorithmsTestHelperTests.Draw(5,6,7,8),
            AlgorithmsTestHelperTests.Draw(1,2,9,10),
        };

        // Act
        var confidence = OddEvenBalanceAlgorithmHelpers.CalculateOddEvenBalanceConfidence(draws, predicted);

        // Assert
        confidence.Should().Be(1.0);
    }

    [Test]
    public void Given_Partial_Matches_When_CalculateOddEvenBalanceConfidence_Method_Is_Invoked_Should_Return_Half()
    {
        // Arrange
        var predicted = new List<int> { 1, 2 };
        var draws = new[]
        {
            AlgorithmsTestHelperTests.Draw(3,4),
            AlgorithmsTestHelperTests.Draw(5,7)
        };

        // Act
        var confidence = OddEvenBalanceAlgorithmHelpers.CalculateOddEvenBalanceConfidence(draws, predicted);

        // Assert
        confidence.Should().BeApproximately(0.5, 1e-9);
    }
}