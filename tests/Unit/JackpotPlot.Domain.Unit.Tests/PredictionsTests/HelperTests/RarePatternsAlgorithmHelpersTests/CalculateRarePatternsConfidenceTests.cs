using FluentAssertions;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.RarePatternsAlgorithmHelpersTests;

[TestFixture]
public class CalculateRarePatternsConfidenceTests
{
    // ---------- helpers ----------
    private static HistoricalDraw Draw(int id, params int[] main) =>
        new HistoricalDraw(
            DrawId: id,
            LotteryId: 1,
            DrawDate: DateTime.UtcNow.AddDays(id),
            WinningNumbers: main.ToList(),
            BonusNumbers: new List<int>(),
            CreatedAt: DateTime.UtcNow.AddDays(id));

    [Test]
    public void Given_Empty_Predicted_When_CalculateRarePatternsConfidence_Method_Is_Invoked_Should_Return_Zero()
    {
        // Arrange
        var draws = new[] { Draw(1, 1, 2, 9) };
        var patterns = RarePatternsAlgorithmHelpers.AnalyzeRarePatterns(draws, numberRange: 10);

        // Act
        var conf = RarePatternsAlgorithmHelpers.CalculateRarePatternsConfidence(draws, new List<int>(), patterns, numberRange: 10);

        // Assert
        conf.Should().Be(0.0);
    }

    [Test]
    public void Given_Empty_Map_When_CalculateRarePatternsConfidence_Method_Is_Invoked_Should_Return_Zero()
    {
        // Arrange
        var draws = new[] { Draw(1, 1, 2, 9) };

        // Act
        var conf = RarePatternsAlgorithmHelpers.CalculateRarePatternsConfidence(draws, [1, 2, 9], new Dictionary<string, int>(), numberRange: 10);

        // Assert
        conf.Should().Be(0.0);
    }

    [Test]
    public void Given_Matching_Rare_Pattern_When_CalculateRarePatternsConfidence_Method_Is_Invoked_Should_Return_Inverse_Frequency_Score()
    {
        // Arrange
        var draws = new[] { Draw(1, 1, 2, 9), Draw(2, 6, 8, 10) };
        var patterns = RarePatternsAlgorithmHelpers.AnalyzeRarePatterns(draws, numberRange: 10); // "2L1H-2O1E" freq=1
        var predicted = new List<int> { 1, 2, 9 };

        // Act
        var conf = RarePatternsAlgorithmHelpers.CalculateRarePatternsConfidence(draws, predicted, patterns, numberRange: 10);

        // Assert
        conf.Should().BeApproximately(0.5, 1e-9); // 1/(1+1)
    }

    [Test]
    public void Given_Unseen_Pattern_When_CalculateRarePatternsConfidence_Method_Is_Invoked_Should_Return_One()
    {
        // Arrange
        var draws = new[] { Draw(1, 1, 2, 9) };
        var patterns = RarePatternsAlgorithmHelpers.AnalyzeRarePatterns(draws, numberRange: 10); // only "2L1H-2O1E" present
        var predicted = new List<int> { 6, 8, 10 }; // "0L3H-0O3E" not in map

        // Act
        var conf = RarePatternsAlgorithmHelpers.CalculateRarePatternsConfidence(draws, predicted, patterns, numberRange: 10);

        // Assert
        conf.Should().Be(1.0);
    }
}