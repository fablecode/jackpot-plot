using FluentAssertions;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.RarePatternsAlgorithmHelpersTests;

[TestFixture]
public class AnalyzeRarePatternsTests
{
    [Test]
    public void Given_Draws_When_AnalyzeRarePatterns_Method_Is_Invoked_Should_Group_By_Pattern_String()
    {
        // Arrange
        var draws = new[] { Draw(1, 1, 2, 9) };

        // Act
        var map = RarePatternsAlgorithmHelpers.AnalyzeRarePatterns(draws, numberRange: 10);

        // Assert
        map.Should().ContainKey("2L1H-2O1E");
    }

    [Test]
    public void Given_Mixed_Frequencies_When_AnalyzeRarePatterns_Method_Is_Invoked_Should_Order_Rarest_First()
    {
        // Arrange
        var draws = new[]
        {
                Draw(1, 1, 2, 9), // "2L1H-2O1E" once  → rarer
                Draw(2, 6, 8, 10),// "0L3H-0O3E" twice → more common
                Draw(3, 6, 8, 10)
            };

        // Act
        var map = RarePatternsAlgorithmHelpers.AnalyzeRarePatterns(draws, numberRange: 10);

        // Assert
        map.First().Key.Should().Be("2L1H-2O1E");
    }

    // ---------- helpers ----------
    private static HistoricalDraw Draw(int id, params int[] main) =>
        new HistoricalDraw(
            DrawId: id,
            LotteryId: 1,
            DrawDate: DateTime.UtcNow.AddDays(id),
            WinningNumbers: main.ToList(),
            BonusNumbers: new List<int>(),
            CreatedAt: DateTime.UtcNow.AddDays(id));
}