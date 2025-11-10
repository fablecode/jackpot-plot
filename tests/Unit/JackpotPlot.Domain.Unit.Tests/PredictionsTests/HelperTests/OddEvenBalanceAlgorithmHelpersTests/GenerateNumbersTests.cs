using FluentAssertions;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.OddEvenBalanceAlgorithmHelpersTests;

[TestFixture]
public class GenerateNumbersTests
{
    [Test]
    public void Given_Non_Positive_Count_When_GenerateNumbers_Method_Is_Invoked_Should_Return_Empty()
    {
        // Arrange
        var rng = new Random(1);

        // Act
        var result = OddEvenBalanceAlgorithmHelpers.GenerateNumbers(1, 10, n => (n & 1) == 1, 0, rng);

        // Assert
        result.IsEmpty.Should().BeTrue();
    }

    [Test]
    public void Given_Odd_Predicate_When_GenerateNumbers_Method_Is_Invoked_Should_Return_Odds_In_Range_And_Distinct()
    {
        // Arrange
        var rng = new Random(2);

        // Act
        var result = OddEvenBalanceAlgorithmHelpers.GenerateNumbers(1, 10, n => (n & 1) == 1, 4, rng);

        // Assert
        (result.All(n => n is >= 1 and <= 10 && (n & 1) == 1) &&
         result.Distinct().Count() == result.Length).Should().BeTrue();
    }

    [Test]
    public void Given_Pool_Smaller_Than_Count_When_GenerateNumbers_Method_Is_Invoked_Should_Return_All_From_Pool()
    {
        // Arrange
        var rng = new Random(3);

        // Act
        var result = OddEvenBalanceAlgorithmHelpers.GenerateNumbers(2, 4, n => (n & 1) == 1, 5, rng);

        // Assert
        result.Length.Should().Be(1);
    }
}