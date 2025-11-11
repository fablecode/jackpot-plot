using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;
using FluentAssertions;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.RepeatingNumbersAlgorithmHelpersTests;

[TestFixture]
public class GetRecentNumbersTests
{
    private static HistoricalDraw Draw(params int[] numbers) =>
            new(
                DrawId: 0,
                LotteryId: 1,
                DrawDate: DateTime.UtcNow,
                WinningNumbers: numbers.ToList(),
                BonusNumbers: new List<int>(),
                CreatedAt: DateTime.UtcNow
            );

    [Test]
    public void Given_History_When_GetRecentNumbers_Is_Invoked_With_Take_2_Should_Return_Numbers_From_First_Two_Draws_Flattened()
    {
        // Arrange
        var history = new List<HistoricalDraw>
            {
                Draw(1, 2, 3),
                Draw(4, 5),
                Draw(6, 7, 8, 9)
            };

        // Act
        var result = RepeatingNumbersAlgorithmHelpers.GetRecentNumbers(history, take: 2);

        // Assert
        result.Should().BeEquivalentTo([1, 2, 3, 4, 5], o => o.WithoutStrictOrdering());
    }

    [Test]
    public void Given_Empty_History_When_GetRecentNumbers_Is_Invoked_Should_Return_Empty_List()
    {
        // Arrange
        var history = new List<HistoricalDraw>();

        // Act
        var result = RepeatingNumbersAlgorithmHelpers.GetRecentNumbers(history, take: 3);

        // Assert
        result.Should().BeEmpty();
    }
}