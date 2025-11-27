using FluentAssertions;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.TimeDecayAlgorithmHelpersTests;

[TestFixture]
public class TimeDecayConfidenceTests
{
    [Test]
    public void Given_Empty_Predicted_When_TimeDecayConfidence_Is_Invoked_Should_Return_Zero()
    {
        // Arrange
        var history = new List<HistoricalDraw>
        {
            Draw(1, new DateTime(2024, 1, 1), 1, 2, 3)
        };
        var predicted = new List<int>();

        // Act
        var score = TimeDecayAlgorithmHelpers.TimeDecayConfidence(history, predicted);

        // Assert
        score.Should().Be(0d);
    }

    [Test]
    public void Given_No_History_When_TimeDecayConfidence_Is_Invoked_Should_Return_Zero()
    {
        // Arrange
        var history = new List<HistoricalDraw>();
        var predicted = new List<int> { 1, 2, 3 };

        // Act
        var score = TimeDecayAlgorithmHelpers.TimeDecayConfidence(history, predicted);

        // Assert
        score.Should().Be(0d);
    }

    [Test]
    public void Given_Two_Recent_Draws_With_Matches_When_TimeDecayConfidence_Is_Invoked_Should_Return_Match_Ratio()
    {
        // Arrange
        var d1 = Draw(1, new DateTime(2024, 1, 10), 1, 2, 3);
        var d2 = Draw(2, new DateTime(2024, 1, 9), 3, 4, 5);
        var history = new List<HistoricalDraw> { d1, d2 };
        var predicted = new List<int> { 2, 3, 9 }; // matches: d1→2,3 (2); d2→3 (1) => total 3; denom=2*3=6

        // Act
        var score = TimeDecayAlgorithmHelpers.TimeDecayConfidence(history, predicted);

        // Assert
        score.Should().Be(3d / 6d);
    }

    private static HistoricalDraw Draw(
        int drawId,
        DateTime drawDate,
        params int[] main)
        => new(
            DrawId: drawId,
            LotteryId: 1,
            DrawDate: drawDate,
            WinningNumbers: main.ToList(),
            BonusNumbers: new List<int>(),
            CreatedAt: drawDate
        );

}