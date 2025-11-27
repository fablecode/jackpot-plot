using FluentAssertions;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.WeightDistributionAlgorithmHelpersTests;

[TestFixture]
public class CalculateWeightDistributionConfidenceTests
{
    [Test]
    public void Given_Empty_History_When_CalculateWeightDistributionConfidence_Is_Invoked_Should_Return_Zero()
    {
        // Arrange
        var history = new List<HistoricalDraw>();
        var predicted = new List<int> { 1, 2, 3 };

        // Act
        var confidence = WeightDistributionAlgorithmHelpers.CalculateWeightDistributionConfidence(history, predicted);

        // Assert
        confidence.Should().Be(0d);
    }

    [Test]
    public void Given_Empty_Predicted_When_CalculateWeightDistributionConfidence_Is_Invoked_Should_Return_Zero()
    {
        // Arrange
        var history = new List<HistoricalDraw> { Draw(1, 2, 3) };
        var predicted = new List<int>();

        // Act
        var confidence = WeightDistributionAlgorithmHelpers.CalculateWeightDistributionConfidence(history, predicted);

        // Assert
        confidence.Should().Be(0d);
    }

    [Test]
    public void Given_Matching_Draws_When_CalculateWeightDistributionConfidence_Is_Invoked_Should_Return_Expected_Ratio()
    {
        // Arrange
        // Draws: [1,2,3] and [2,3,4]; predicted: [2,3]
        // matches = 2 (in first) + 2 (in second) = 4; denom = 2 draws * 2 predicted = 4 => 1.0
        var history = new List<HistoricalDraw>
        {
            Draw(1, 2, 3),
            Draw(2, 3, 4)
        };
        var predicted = new List<int> { 2, 3 };

        // Act
        var confidence = WeightDistributionAlgorithmHelpers.CalculateWeightDistributionConfidence(history, predicted);

        // Assert
        confidence.Should().Be(1d);
    }

    private static HistoricalDraw Draw(params int[] numbers) =>
        new(
            DrawId: 1,
            LotteryId: 1,
            DrawDate: DateTime.UtcNow,
            WinningNumbers: numbers.ToList(),
            BonusNumbers: new List<int>(),
            CreatedAt: DateTime.UtcNow
        );
}