using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;
using FluentAssertions;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.ReducedNumberPoolAlgorithmHelpersTests;

[TestFixture]
public class AnalyzeReducedNumberPoolTests
{
    [Test]
    public void Given_No_History_When_AnalyzeReducedNumberPool_Is_Invoked_Should_Return_Full_Range()
    {
        // Arrange
        var draws = Array.Empty<HistoricalDraw>();

        // Act
        var pool = ReducedNumberPoolAlgorithmHelpers.AnalyzeReducedNumberPool(draws, numberRange: 10, thresholdRatio: 0.1);

        // Assert
        pool.Should().BeEquivalentTo(Enumerable.Range(1, 10));
    }

    [Test]
    public void Given_Low_Threshold_When_AnalyzeReducedNumberPool_Is_Invoked_Should_Include_Numbers_Meeting_Threshold()
    {
        // Arrange
        var draws = Enumerable.Range(1, 10).Select(i => Draw(i, i)).ToList();

        // Act
        var pool = ReducedNumberPoolAlgorithmHelpers.AnalyzeReducedNumberPool(draws, numberRange: 10, thresholdRatio: 0.1);

        // Assert
        pool.Should().BeEquivalentTo(Enumerable.Range(1, 10));
    }

    [Test]
    public void Given_High_Threshold_When_AnalyzeReducedNumberPool_Is_Invoked_Should_Fallback_To_Full_Range_When_Empty()
    {
        // Arrange
        var draws = new List<HistoricalDraw> { Draw(1, 1), Draw(2, 2), Draw(3, 3), Draw(4, 4), Draw(5, 5) };

        // Act
        var pool = ReducedNumberPoolAlgorithmHelpers.AnalyzeReducedNumberPool(draws, numberRange: 6, thresholdRatio: 2.0);

        // Assert
        pool.Should().BeEquivalentTo(Enumerable.Range(1, 6));
    }

    [Test]
    public void Given_Mixed_Frequencies_When_AnalyzeReducedNumberPool_Is_Invoked_Should_Filter_By_Threshold()
    {
        // Arrange
        var draws = new List<HistoricalDraw>
            {
                Draw(1, 1, 2),
                Draw(2, 1, 3),
                Draw(3, 1, 4),
                Draw(4, 5),
                Draw(5, 6)
            };

        // Act
        var pool = ReducedNumberPoolAlgorithmHelpers.AnalyzeReducedNumberPool(draws, numberRange: 6, thresholdRatio: 0.6);

        // Assert
        pool.Should().BeEquivalentTo([1]);
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