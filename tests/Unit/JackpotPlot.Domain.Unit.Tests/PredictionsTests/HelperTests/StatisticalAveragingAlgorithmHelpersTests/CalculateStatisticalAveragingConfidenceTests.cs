using System.Collections.Immutable;
using FluentAssertions;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.StatisticalAveragingAlgorithmHelpersTests;

[TestFixture]
public class CalculateStatisticalAveragingConfidenceTests
{
    [Test]
    public void Given_Empty_History_When_CalculateStatisticalAveragingConfidence_Is_Invoked_Should_Return_Zero()
    {
        // Arrange
        var history = new List<HistoricalDraw>();
        var predicted = ImmutableArray.Create(1, 2, 3);

        // Act
        var score = StatisticalAveragingAlgorithmHelpers.CalculateStatisticalAveragingConfidence(history, predicted);

        // Assert
        score.Should().Be(0d);
    }

    [Test]
    public void Given_Empty_Predictions_When_CalculateStatisticalAveragingConfidence_Is_Invoked_Should_Return_Zero()
    {
        // Arrange
        var history = new List<HistoricalDraw> { Draw([2, 2]) };
        var predicted = ImmutableArray<int>.Empty;

        // Act
        var score = StatisticalAveragingAlgorithmHelpers.CalculateStatisticalAveragingConfidence(history, predicted);

        // Assert
        score.Should().Be(0d);
    }

    [Test]
    public void Given_Matching_Averages_When_CalculateStatisticalAveragingConfidence_Is_Invoked_Should_Return_One()
    {
        // Arrange
        var history = new List<HistoricalDraw>
        {
            Draw([2, 2]), Draw([2, 2])
        };
        var predicted = ImmutableArray.Create(2, 2);

        // Act
        var score = StatisticalAveragingAlgorithmHelpers.CalculateStatisticalAveragingConfidence(history, predicted);

        // Assert
        score.Should().Be(1d);
    }

    [Test]
    public void Given_Predicted_Average_Offset_By_Two_When_CalculateStatisticalAveragingConfidence_Is_Invoked_Should_Return_One_Third()
    {
        // Arrange
        var history = new List<HistoricalDraw>
        {
            Draw([5, 5]), Draw([5, 5])
        };
        var predicted = ImmutableArray.Create(7, 7);

        // Act
        var score = StatisticalAveragingAlgorithmHelpers.CalculateStatisticalAveragingConfidence(history, predicted);

        // Assert
        score.Should().Be(1d / 3d);
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