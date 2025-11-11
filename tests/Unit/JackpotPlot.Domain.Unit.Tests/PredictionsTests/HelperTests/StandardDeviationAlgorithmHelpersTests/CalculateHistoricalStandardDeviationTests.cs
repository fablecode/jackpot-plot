using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;
using FluentAssertions;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.StandardDeviationAlgorithmHelpersTests;

[TestFixture]
public class CalculateHistoricalStandardDeviationTests
{
    private static HistoricalDraw Draw(params int[] numbers) =>
            new(
                DrawId: 1,
                LotteryId: 1,
                DrawDate: DateTime.UtcNow,
                WinningNumbers: numbers.ToList(),
                BonusNumbers: new List<int>(),
                CreatedAt: DateTime.UtcNow);

    [Test]
    public void Given_Empty_Draws_When_CalculateHistoricalStandardDeviation_Is_Invoked_Should_Return_Zero()
    {
        // Arrange
        var draws = new List<HistoricalDraw>();

        // Act
        var sd = StandardDeviationAlgorithmHelpers.CalculateHistoricalStandardDeviation(draws);

        // Assert
        sd.Should().Be(0d);
    }

    [Test]
    public void Given_Known_Data_When_CalculateHistoricalStandardDeviation_Is_Invoked_Should_Return_Expected()
    {
        // Arrange
        var draws = new List<HistoricalDraw> { Draw(1), Draw(3) };

        // Act
        var sd = StandardDeviationAlgorithmHelpers.CalculateHistoricalStandardDeviation(draws);

        // Assert
        sd.Should().Be(1d);
    }
}