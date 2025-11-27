using FluentAssertions;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.TimeDecayAlgorithmHelpersTests;

[TestFixture]
public class AssignTimeDecayWeightsTests
{
    [Test]
    public void Given_Draws_With_Different_Dates_When_AssignTimeDecayWeights_Is_Invoked_Should_Assign_Higher_Weight_To_More_Recent()
    {
        // Arrange
        var older = Draw(1, new DateTime(2024, 1, 1), 1, 2);
        var newer = Draw(2, new DateTime(2024, 1, 10), 3, 4);
        var draws = new[] { older, newer };
        const double decay = 0.5d;

        // Act
        var weights = TimeDecayAlgorithmHelpers.AssignTimeDecayWeights(draws, decay);

        // Assert
        (weights[2] > weights[1]).Should().BeTrue();
    }

    [Test]
    public void Given_Decay_Factor_When_AssignTimeDecayWeights_Is_Invoked_Should_Use_Powers_Of_Decay()
    {
        // Arrange
        var d1 = Draw(1, new DateTime(2024, 1, 1), 1);
        var d2 = Draw(2, new DateTime(2024, 1, 2), 2);
        var d3 = Draw(3, new DateTime(2024, 1, 3), 3);
        var draws = new[] { d1, d2, d3 };
        var decay = 0.5d;

        // Act
        var weights = TimeDecayAlgorithmHelpers.AssignTimeDecayWeights(draws, decay);

        // Assert
        (Math.Abs(weights[3] - 1d) < 1e-9
         && Math.Abs(weights[2] - 0.5d) < 1e-9
         && Math.Abs(weights[1] - 0.25d) < 1e-9).Should().BeTrue();
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