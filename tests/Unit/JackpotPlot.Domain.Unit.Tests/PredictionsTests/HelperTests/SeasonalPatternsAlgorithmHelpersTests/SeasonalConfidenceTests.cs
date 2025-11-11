using FluentAssertions;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.SeasonalPatternsAlgorithmHelpersTests;

[TestFixture]
public class SeasonalConfidenceTests
{
    [Test]
    public void Given_No_Seasonal_Draws_When_SeasonalConfidence_Is_Invoked_Should_Return_Zero()
    {
        // Arrange
        var history = new List<HistoricalDraw>
        {
            Draw(new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), 1,2,3) // Winter
        };
        var predicted = new List<int> { 1, 2, 3 };

        // Act
        var confidence = SeasonalPatternsAlgorithmHelpers.SeasonalConfidence(history, predicted, "Summer");

        // Assert
        confidence.Should().Be(0d);
    }

    [Test]
    public void Given_Seasonal_Draws_When_SeasonalConfidence_Is_Invoked_Should_Return_Match_Ratio()
    {
        // Arrange
        var history = new List<HistoricalDraw>
        {
            Draw(new DateTime(2025, 9, 1, 0, 0, 0, DateTimeKind.Utc), 1, 2),  // Fall
            Draw(new DateTime(2025, 10, 1, 0, 0, 0, DateTimeKind.Utc), 2, 3)  // Fall
        };
        var predicted = new List<int> { 2, 9, 10 }; // matches across Fall draws: 1st->1, 2nd->1 => total 2, denom=2*3=6 => 2/6

        // Act
        var confidence = SeasonalPatternsAlgorithmHelpers.SeasonalConfidence(history, predicted, "Fall");

        // Assert
        confidence.Should().Be(2d / 6d);
    }

    [Test]
    public void Given_Empty_Predictions_When_SeasonalConfidence_Is_Invoked_Should_Return_Zero()
    {
        // Arrange
        var history = new List<HistoricalDraw> { Draw(new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc), 1, 2) };

        // Act
        var confidence = SeasonalPatternsAlgorithmHelpers.SeasonalConfidence(history, new List<int>(), "Summer");

        // Assert
        confidence.Should().Be(0d);
    }

    private static HistoricalDraw Draw(DateTime dateUtc, params int[] numbers) =>
        new(
            DrawId: 1,
            LotteryId: 1,
            DrawDate: dateUtc,
            WinningNumbers: numbers.ToList(),
            BonusNumbers: new List<int>(),
            CreatedAt: dateUtc
        );
}