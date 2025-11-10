using FluentAssertions;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.RarePatternsAlgorithmHelpersTests;

[TestFixture]
public class GenerateFromRarestPatternTests
{
    [Test]
    public void Given_Empty_Input_When_GenerateFromRarestPattern_Method_Is_Invoked_Should_Return_Empty()
    {
        // Arrange
        var rng = new Random(1);

        // Act
        var result = RarePatternsAlgorithmHelpers.GenerateFromRarestPattern(
            rarePatterns: new Dictionary<string, int>(),
            count: 5,
            numberRange: 10,
            rng: rng);

        // Assert
        result.IsEmpty.Should().BeTrue();
    }

    [Test]
    public void Given_Non_Positive_Count_When_GenerateFromRarestPattern_Method_Is_Invoked_Should_Return_Empty()
    {
        // Arrange
        var rng = new Random(2);
        var patterns = new Dictionary<string, int> { ["2L1H-2O1E"] = 1 };

        // Act
        var result = RarePatternsAlgorithmHelpers.GenerateFromRarestPattern(patterns, 0, 10, rng);

        // Assert
        result.IsEmpty.Should().BeTrue();
    }

    [Test]
    public void Given_Rarest_Pattern_When_GenerateFromRarestPattern_Method_Is_Invoked_Should_Return_Requested_Count()
    {
        // Arrange
        var rng = new Random(3);
        var patterns = new Dictionary<string, int> { ["2L1H-2O1E"] = 1 };

        // Act
        var result = RarePatternsAlgorithmHelpers.GenerateFromRarestPattern(patterns, count: 3, numberRange: 10, rng: rng);

        // Assert
        result.Length.Should().Be(3);
    }

    [Test]
    public void Given_Rarest_Pattern_When_GenerateFromRarestPattern_Method_Is_Invoked_Should_Return_In_Range_And_Distinct()
    {
        // Arrange
        var rng = new Random(5);
        var patterns = new Dictionary<string, int> { ["1L2H-1O2E"] = 1 };

        // Act
        var result = RarePatternsAlgorithmHelpers.GenerateFromRarestPattern(patterns, count: 3, numberRange: 12, rng: rng);

        // Assert
        (result.All(n => n is >= 1 and <= 12) && result.Distinct().Count() == result.Length).Should().BeTrue();
    }
}