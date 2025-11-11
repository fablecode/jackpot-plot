using FluentAssertions;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.StandardDeviationAlgorithmHelpersTests;

[TestFixture]
public class CalculateStandardDeviationConfidenceTests
{
    [Test]
    public void Given_Empty_Draws_When_CalculateStandardDeviationConfidence_Is_Invoked_Should_Return_Zero()
    {
        // Arrange
        var draws = new List<HistoricalDraw>();
        var predicted = new List<int> { 1, 2, 3 };

        // Act
        var score = StandardDeviationAlgorithmHelpers.CalculateStandardDeviationConfidence(draws, predicted);

        // Assert
        score.Should().Be(0d);
    }

    [Test]
    public void Given_Empty_Predicted_When_CalculateStandardDeviationConfidence_Is_Invoked_Should_Return_Zero()
    {
        // Arrange
        var draws = new List<HistoricalDraw> { Draw(1, 3) };
        var predicted = new List<int>();

        // Act
        var score = StandardDeviationAlgorithmHelpers.CalculateStandardDeviationConfidence(draws, predicted);

        // Assert
        score.Should().Be(0d);
    }

    [Test]
    public void Given_Matching_StdDev_When_CalculateStandardDeviationConfidence_Is_Invoked_Should_Return_One()
    {
        // Arrange
        var draws = new List<HistoricalDraw> { Draw(1), Draw(3) };
        var predicted = new List<int> { 1, 3 };                   

        // Act
        var score = StandardDeviationAlgorithmHelpers.CalculateStandardDeviationConfidence(draws, predicted);

        // Assert
        score.Should().Be(1d);
    }

    private static HistoricalDraw Draw(params int[] numbers) =>
        new(
            DrawId: 1,
            LotteryId: 1,
            DrawDate: DateTime.UtcNow,
            WinningNumbers: numbers.ToList(),
            BonusNumbers: new List<int>(),
            CreatedAt: DateTime.UtcNow);
}