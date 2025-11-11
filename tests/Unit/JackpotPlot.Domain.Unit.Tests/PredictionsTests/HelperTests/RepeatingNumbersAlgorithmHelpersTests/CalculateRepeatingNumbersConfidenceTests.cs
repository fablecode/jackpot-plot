using FluentAssertions;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.RepeatingNumbersAlgorithmHelpersTests;

[TestFixture]
public class CalculateRepeatingNumbersConfidenceTests
{
    [Test]
    public void Given_Empty_Draws_When_CalculateRepeatingNumbersConfidence_Is_Invoked_Should_Return_Zero()
    {
        // Arrange
        var draws = Array.Empty<HistoricalDraw>();
        var predicted = new List<int> { 1, 2, 3 };

        // Act
        var confidence = RepeatingNumbersAlgorithmHelpers.CalculateRepeatingNumbersConfidence(draws, predicted);

        // Assert
        confidence.Should().Be(0d);
    }

    [Test]
    public void Given_Empty_Predictions_When_CalculateRepeatingNumbersConfidence_Is_Invoked_Should_Return_Zero()
    {
        // Arrange
        var draws = new List<HistoricalDraw> { Draw(1, 2, 3) };
        var predicted = new List<int>();

        // Act
        var confidence = RepeatingNumbersAlgorithmHelpers.CalculateRepeatingNumbersConfidence(draws, predicted);

        // Assert
        confidence.Should().Be(0d);
    }

    [Test]
    public void Given_Two_Draws_And_Matches_When_CalculateRepeatingNumbersConfidence_Is_Invoked_Should_Return_Match_Ratio()
    {
        // Arrange
        var draws = new List<HistoricalDraw>
        {
            Draw(1, 2, 3),
            Draw(3, 4, 5)
        };
        var predicted = new List<int> { 2, 3, 9 };

        // Act
        var confidence = RepeatingNumbersAlgorithmHelpers.CalculateRepeatingNumbersConfidence(draws, predicted);

        // Assert
        confidence.Should().Be(3d / 6d);
    }

    private static HistoricalDraw Draw(params int[] numbers) =>
        new(
            DrawId: 0,
            LotteryId: 1,
            DrawDate: DateTime.UtcNow,
            WinningNumbers: numbers.ToList(),
            BonusNumbers: new List<int>(),
            CreatedAt: DateTime.UtcNow
        );

}