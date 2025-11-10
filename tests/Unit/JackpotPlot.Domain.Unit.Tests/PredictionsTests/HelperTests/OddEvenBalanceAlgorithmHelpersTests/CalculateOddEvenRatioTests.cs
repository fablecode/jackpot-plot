using FluentAssertions;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.OddEvenBalanceAlgorithmHelpersTests;

[TestFixture]
public class CalculateOddEvenRatioTests
{
    [Test]
    public void Given_No_Draws_When_CalculateOddEvenRatio_Method_Is_Invoked_Should_Return_Equal_Ratios()
    {
        // Arrange
        var draws = Array.Empty<HistoricalDraw>();

        // Act
        var (odd, even) = OddEvenBalanceAlgorithmHelpers.CalculateOddEvenRatio(draws);

        // Assert
        (odd == 0.5 && even == 0.5).Should().BeTrue();
    }

    [Test]
    public void Given_Mixed_Draws_When_CalculateOddEvenRatio_Method_Is_Invoked_Should_Return_Correct_Odd_Ratio()
    {
        // Arrange
        var draws = new[]
        {
                AlgorithmsTestHelperTests.Draw(1,2,3),
                AlgorithmsTestHelperTests.Draw(4,6)
            };

        // Act
        var (odd, _) = OddEvenBalanceAlgorithmHelpers.CalculateOddEvenRatio(draws);

        // Assert
        odd.Should().BeApproximately(0.4, 1e-9);
    }
}