using FluentAssertions;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.SymmetryAnalysisAlgorithmHelpersTests;

[TestFixture]
public class AnalyzeSymmetryMetricsTests
{
    [Test]
    public void Given_Balanced_Draws_When_AnalyzeSymmetryMetrics_Is_Invoked_Should_Return_Ratios_Close_To_One()
    {
        // Arrange
        var draws = new List<HistoricalDraw> { Draw(1, 2, 3, 4, 5, 6) };
        var range = 10;

        // Act
        var (hl, oe) = SymmetryAnalysisAlgorithmHelpers.AnalyzeSymmetryMetrics(draws, range);

        // Assert
        hl.Should().BePositive();
        oe.Should().BePositive();
    }

    [Test]
    public void Given_All_High_Numbers_When_AnalyzeSymmetryMetrics_Is_Invoked_Should_Return_HighLowRatio_Infinite_And_Clamped()
    {
        // Arrange
        var draws = new List<HistoricalDraw> { Draw(8, 9, 10) };
        var range = 10;

        // Act
        var (hl, _) = SymmetryAnalysisAlgorithmHelpers.AnalyzeSymmetryMetrics(draws, range);

        // Assert
        hl.Should().Be(1e6); // clamped from infinity
    }

    [Test]
    public void Given_All_Odd_Numbers_When_AnalyzeSymmetryMetrics_Is_Invoked_Should_Return_OddEvenRatio_Infinite_And_Clamped()
    {
        // Arrange
        var draws = new List<HistoricalDraw> { Draw(1, 3, 5, 7, 9) };
        var range = 10;

        // Act
        var (_, oe) = SymmetryAnalysisAlgorithmHelpers.AnalyzeSymmetryMetrics(draws, range);

        // Assert
        oe.Should().Be(1e6); // clamped
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