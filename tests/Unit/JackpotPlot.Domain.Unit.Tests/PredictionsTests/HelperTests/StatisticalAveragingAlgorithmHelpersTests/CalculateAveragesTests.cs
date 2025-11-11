using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;
using FluentAssertions;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.StatisticalAveragingAlgorithmHelpersTests;

[TestFixture]
public class CalculateAveragesTests
{
    [Test]
    public void Given_Positional_Data_When_CalculateAverages_Is_Invoked_Should_Return_Rounded_Means_Away_From_Zero()
    {
        // Arrange
        var history = new List<HistoricalDraw>
            {
                Draw([1, 3]),
                Draw([2, 4])
            };

        // Act
        var result = StatisticalAveragingAlgorithmHelpers.CalculateAverages(history, numbersCount: 2, maxRange: 50, isBonus: false);

        // Assert
        result.Should().Equal(2, 4);
    }

    [Test]
    public void Given_Values_Beyond_Range_When_CalculateAverages_Is_Invoked_Should_Clamp_To_Range()
    {
        // Arrange
        var history = new List<HistoricalDraw>
            {
                Draw([60]),
                Draw([50])
            };

        // Act
        var result = StatisticalAveragingAlgorithmHelpers.CalculateAverages(history, numbersCount: 1, maxRange: 10, isBonus: false);

        // Assert
        result.Single().Should().Be(10);
    }

    [Test]
    public void Given_Missing_Positions_When_CalculateAverages_Is_Invoked_Should_Backfill_Using_Jitter_Around_Global_Mean()
    {
        // Arrange
        var history = new List<HistoricalDraw>
            {
                // Only position 0 exists across draws
                Draw([10]),
                Draw([10])
            };

        // Act
        var result = StatisticalAveragingAlgorithmHelpers.CalculateAverages(history, numbersCount: 3, maxRange: 50, isBonus: false);

        // Assert
        result.Should().Equal(10, 9, 11);
    }

    [Test]
    public void Given_IsBonus_True_When_CalculateAverages_Is_Invoked_Should_Use_BonusNumbers()
    {
        // Arrange
        var history = new List<HistoricalDraw>
            {
                Draw([1, 2, 3], bonus: [5, 7]),
                Draw([4, 5, 6], bonus: [7, 9])
            };

        // Act
        var result = StatisticalAveragingAlgorithmHelpers.CalculateAverages(history, numbersCount: 2, maxRange: 50, isBonus: true);

        // Assert
        result.Should().Equal(6, 8);
    }

    [Test]
    public void Given_Empty_History_When_CalculateAverages_Is_Invoked_Should_Backfill_From_Default_Global_Mean_With_Clamped_Jitter()
    {
        // Arrange
        var history = new List<HistoricalDraw>();

        // Act
        var result = StatisticalAveragingAlgorithmHelpers.CalculateAverages(history, numbersCount: 3, maxRange: 10, isBonus: false);

        // Assert
        result.Should().Equal(1, 1, 1);
    }

    [Test]
    public void Given_Global_Mean_Close_To_Lower_Bound_When_CalculateAverages_Is_Invoked_Should_Clamp_Jitter_At_One()
    {
        // Arrange
        var history = new List<HistoricalDraw> { Draw([1]), Draw([1]) };

        // Act
        var result = StatisticalAveragingAlgorithmHelpers.CalculateAverages(history, numbersCount: 3, maxRange: 10, isBonus: false);

        // Assert
        result.Should().Equal(1, 1, 2);
    }

    private static HistoricalDraw Draw(IEnumerable<int> main, IEnumerable<int>? bonus = null) =>
        new(
            DrawId: 1,
            LotteryId: 1,
            DrawDate: DateTime.UtcNow,
            WinningNumbers: main.ToList(),
            BonusNumbers: (bonus ?? Array.Empty<int>()).ToList(),
            CreatedAt: DateTime.UtcNow);
}