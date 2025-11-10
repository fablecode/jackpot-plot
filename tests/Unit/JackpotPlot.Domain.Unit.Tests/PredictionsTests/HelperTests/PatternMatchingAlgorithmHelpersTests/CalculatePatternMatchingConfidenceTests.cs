using FluentAssertions;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.PatternMatchingAlgorithmHelpersTests;

[TestFixture]
public class CalculatePatternMatchingConfidenceTests
{
    [Test]
    public void Given_Empty_PredictedPattern_When_CalculatePatternMatchingConfidence_Method_Is_Invoked_Should_Return_Zero()
    {
        // Arrange
        var cfg = Config(mainRange: 10, mainCount: 2);
        var draws = new[] { Draw(1, 1, 6) };

        // Act
        var conf = PatternMatchingAlgorithmHelpers.CalculatePatternMatchingConfidence(draws, "", cfg);

        // Assert
        conf.Should().Be(0.0);
    }

    [Test]
    public void Given_Pattern_When_CalculatePatternMatchingConfidence_Method_Is_Invoked_Should_Return_Match_Ratio()
    {
        // Arrange
        var cfg = Config(mainRange: 10, mainCount: 2);
        var draws = new[]
        {
            Draw(1, 1, 6), // OL,EH
            Draw(2, 3, 8), // OL,EH
            Draw(3, 1, 2)  // OL,EL
        };

        // Act
        var conf = PatternMatchingAlgorithmHelpers.CalculatePatternMatchingConfidence(draws, "OL,EH", cfg);

        // Assert
        conf.Should().BeApproximately(2.0 / 3.0, 1e-9);
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