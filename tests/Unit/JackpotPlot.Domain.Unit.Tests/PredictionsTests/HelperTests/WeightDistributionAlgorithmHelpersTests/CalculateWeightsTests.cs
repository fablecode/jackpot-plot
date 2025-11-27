using FluentAssertions;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.WeightDistributionAlgorithmHelpersTests;

[TestFixture]
public class CalculateWeightsTests
{
    [Test]
    public void Given_Empty_History_When_CalculateWeights_Is_Invoked_Should_Return_Uniform_Distribution()
    {
        // Arrange
        var history = new List<HistoricalDraw>();
        const int range = 10;

        // Act
        var weights = WeightDistributionAlgorithmHelpers.CalculateWeights(history, range);

        // Assert
        var allEqualAndSumOne =
            weights.Count == range &&
            Math.Abs(weights.Values.Sum() - 1d) < 1e-9 &&
            weights.Values.All(v => Math.Abs(v - 1d / range) < 1e-9);

        allEqualAndSumOne.Should().BeTrue();
    }

    [Test]
    public void Given_Draws_With_InRange_Numbers_When_CalculateWeights_Is_Invoked_Should_Weight_By_Frequency()
    {
        // Arrange
        var history = new List<HistoricalDraw>
        {
            Draw(1, 2, 2),
            Draw(2, 3)
        };
        const int range = 3;

        // Act
        var weights = WeightDistributionAlgorithmHelpers.CalculateWeights(history, range);

        // Assert
        // Counts: 1→1, 2→3, 3→1 => total=5 => weights: 0.2, 0.6, 0.2
        var correct =
            Math.Abs(weights[1] - 0.2d) < 1e-9 &&
            Math.Abs(weights[2] - 0.6d) < 1e-9 &&
            Math.Abs(weights[3] - 0.2d) < 1e-9;

        correct.Should().BeTrue();
    }

    [Test]
    public void Given_Draws_With_OutOfRange_Numbers_When_CalculateWeights_Is_Invoked_Should_Ignore_OutOfRange()
    {
        // Arrange
        var history = new List<HistoricalDraw>
            {
                Draw(0, 1, 5, 6) // with range=5, 0 and 6 should be ignored
            };
        const int range = 5;

        // Act
        var weights = WeightDistributionAlgorithmHelpers.CalculateWeights(history, range);

        // Assert
        // Only 1 and 5 are counted -> each should get 0.5
        var correct =
            Math.Abs(weights[1] - 0.5d) < 1e-9 &&
            Math.Abs(weights[5] - 0.5d) < 1e-9 &&
            Math.Abs(weights[2] + weights[3] + weights[4]) < 1e-9;

        correct.Should().BeTrue();
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