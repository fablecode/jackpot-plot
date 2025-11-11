using FluentAssertions;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.SeasonalPatternsAlgorithmHelpersTests;

[TestFixture]
public class AnalyzeSeasonalFrequenciesTests
{
    [Test]
    public void Given_Draws_In_Mixed_Seasons_When_AnalyzeSeasonalFrequencies_Is_Invoked_Should_Count_Only_Selected_Season()
    {
        // Arrange
        var history = new List<HistoricalDraw>
        {
            Draw(new DateTime(2025, 1, 5, 0, 0, 0, DateTimeKind.Utc), 1,2,3),   // Winter
            Draw(new DateTime(2025, 4, 5, 0, 0, 0, DateTimeKind.Utc), 2,3,4),   // Spring
            Draw(new DateTime(2025, 4, 12, 0, 0, 0, DateTimeKind.Utc), 3,5)     // Spring
        };

        // Act
        var freq = SeasonalPatternsAlgorithmHelpers.AnalyzeSeasonalFrequencies(history, "Spring", numberRange: 10);

        // Assert
        freq[3].Should().Be(2);
    }

    [Test]
    public void Given_Numbers_Outside_Range_When_AnalyzeSeasonalFrequencies_Is_Invoked_Should_Ignore_Out_Of_Range()
    {
        // Arrange
        var history = new List<HistoricalDraw>
        {
            Draw(new DateTime(2025, 7, 1, 0, 0, 0, DateTimeKind.Utc), 0, 1, 11) // Summer; 0 and 11 out of [1..10]
        };

        // Act
        var freq = SeasonalPatternsAlgorithmHelpers.AnalyzeSeasonalFrequencies(history, "Summer", numberRange: 10);

        // Assert
        freq[1].Should().Be(1);
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