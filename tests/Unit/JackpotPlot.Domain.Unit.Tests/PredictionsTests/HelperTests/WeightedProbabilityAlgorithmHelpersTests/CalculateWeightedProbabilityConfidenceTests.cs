using FluentAssertions;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.WeightedProbabilityAlgorithmHelpersTests;

[TestFixture]
public class CalculateWeightedProbabilityConfidenceTests
{
    [Test]
    public void Given_Empty_History_When_CalculateWeightedProbabilityConfidence_Is_Invoked_Should_Return_Zero()
    {
        // Arrange
        var history = new List<HistoricalDraw>();
        var weights = new Dictionary<int, double> { { 1, 0.5 }, { 2, 0.5 } };

        // Act
        var confidence = WeightedProbabilityAlgorithmHelpers.CalculateWeightedProbabilityConfidence(history, weights);

        // Assert
        confidence.Should().Be(0d);
    }

    [Test]
    public void Given_History_And_Weights_When_CalculateWeightedProbabilityConfidence_Is_Invoked_Should_Normalize_By_Total_Mass()
    {
        // Arrange
        var history = new List<HistoricalDraw>
        {
            Draw(1, 2), Draw(2, 3)
        };
        var weights = new Dictionary<int, double>
        {
            { 1, 0.2 },
            { 2, 0.3 },
            { 3, 0.5 }
        };

        // matched = w1 + w2 + w2 + w3 = 0.2 + 0.3 + 0.3 + 0.5 = 1.3
        // totalMass = draws(2) * sum(weights)(1.0) = 2.0 => 1.3 / 2 = 0.65

        // Act
        var confidence = WeightedProbabilityAlgorithmHelpers.CalculateWeightedProbabilityConfidence(history, weights);

        // Assert
        Math.Abs(confidence - 0.65d).Should().BeLessThan(1e-9);
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