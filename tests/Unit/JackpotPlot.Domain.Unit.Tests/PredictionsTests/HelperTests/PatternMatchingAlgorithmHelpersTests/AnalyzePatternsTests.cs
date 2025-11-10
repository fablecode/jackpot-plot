using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;
using FluentAssertions;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.PatternMatchingAlgorithmHelpersTests;

[TestFixture]
public class AnalyzePatternsTests
{
    [Test]
    public void Given_Draws_With_Expected_Count_When_AnalyzePatterns_Method_Is_Invoked_Should_Count_Patterns()
    {
        // Arrange
        var cfg = Config(mainRange: 10, mainCount: 2);

        var draws = new[]
        {
                Draw(1, 1, 6),
                Draw(2, 1, 6),
                Draw(3, 1) // ignored (wrong count)
            };

        // Act
        var map = PatternMatchingAlgorithmHelpers.AnalyzePatterns(draws, cfg);

        // Assert
        map["OL,EH"].Should().Be(2);
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

    private static LotteryConfigurationDomain Config(
        int mainRange = 10,
        int mainCount = 2)
    {
        return new LotteryConfigurationDomain
        {
            LotteryId = new Random(12345).Next(),
            MainNumbersRange = mainRange,
            MainNumbersCount = mainCount,
            BonusNumbersRange = 0,
            BonusNumbersCount = 0
        };
    }
}