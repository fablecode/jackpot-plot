using FluentAssertions;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.TimeDecayAlgorithmHelpersTests;

[TestFixture]
public class CalculateWeightedFrequencies
{
    [Test]
    public void Given_Weights_And_Draws_When_CalculateWeightedFrequencies_Is_Invoked_Should_Sum_Weighted_Occurrences()
    {
        // Arrange
        var d1 = Draw(1, new DateTime(2024, 1, 1), 1, 2);
        var d2 = Draw(2, new DateTime(2024, 1, 2), 2, 3);
        var draws = new[] { d1, d2 };
        var weights = new Dictionary<int, double>
        {
            [1] = 1.0,
            [2] = 2.0
        };

        // Act
        var freq = TimeDecayAlgorithmHelpers.CalculateWeightedFrequencies(draws, weights, range: 5);

        // Assert
        (Math.Abs(freq[1] - 1.0) < 1e-9   // 1 only in draw1 * 1.0
         && Math.Abs(freq[2] - 3.0) < 1e-9 // 2 in draw1 + draw2 => 1.0 + 2.0
         && Math.Abs(freq[3] - 2.0) < 1e-9 // 3 only in draw2 * 2.0
            ).Should().BeTrue();
    }

    [Test]
    public void Given_Numbers_Outside_Range_When_CalculateWeightedFrequencies_Is_Invoked_Should_Ignore_Out_Of_Range()
    {
        // Arrange
        var d1 = Draw(1, new DateTime(2024, 1, 1), 0, 1, 6); // 0 and 6 are out of [1..5]
        var draws = new[] { d1 };
        var weights = new Dictionary<int, double> { [1] = 1.0 };

        // Act
        var freq = TimeDecayAlgorithmHelpers.CalculateWeightedFrequencies(draws, weights, range: 5);

        // Assert
        (freq[1] == 1.0 && freq[2] == 0.0 && freq[5] == 0.0).Should().BeTrue();
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