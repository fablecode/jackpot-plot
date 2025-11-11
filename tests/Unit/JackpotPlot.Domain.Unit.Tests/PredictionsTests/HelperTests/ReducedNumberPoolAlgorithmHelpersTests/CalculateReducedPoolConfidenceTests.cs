using FluentAssertions;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.ReducedNumberPoolAlgorithmHelpersTests;

[TestFixture]
public class CalculateReducedPoolConfidenceTests
{
    [Test]
    public void Given_Empty_Predicted_When_CalculateReducedPoolConfidence_Is_Invoked_Should_Return_Zero()
    {
        // Arrange
        var draws = new List<HistoricalDraw> { Draw(1, 1, 2, 3) };

        // Act
        var conf = ReducedNumberPoolAlgorithmHelpers.CalculateReducedPoolConfidence(draws, new List<int>());

        // Assert
        conf.Should().Be(0.0);
    }

    [Test]
    public void Given_Empty_History_When_CalculateReducedPoolConfidence_Is_Invoked_Should_Return_Zero()
    {
        // Arrange
        var draws = new List<HistoricalDraw>();
        var predicted = new List<int> { 1, 2 };

        // Act
        var conf = ReducedNumberPoolAlgorithmHelpers.CalculateReducedPoolConfidence(draws, predicted);

        // Assert
        conf.Should().Be(0.0);
    }

    [Test]
    public void Given_Known_Overlap_When_CalculateReducedPoolConfidence_Is_Invoked_Should_Return_Expected_Ratio()
    {
        // Arrange
        var draws = new List<HistoricalDraw> { Draw(1, 1, 2, 3), Draw(2, 2, 4) };
        var predicted = new List<int> { 1, 2 };

        // Act
        var conf = ReducedNumberPoolAlgorithmHelpers.CalculateReducedPoolConfidence(draws, predicted);

        // Assert
        conf.Should().BeApproximately(0.75, 1e-9);
    }

    private static HistoricalDraw Draw(int id, params int[] main) =>
        new HistoricalDraw(
            DrawId: id,
            LotteryId: 1,
            DrawDate: DateTime.UtcNow.AddDays(id),
            WinningNumbers: main.ToList(),
            BonusNumbers: new List<int>(),
            CreatedAt: DateTime.UtcNow.AddDays(id));
}