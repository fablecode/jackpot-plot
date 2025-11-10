using FluentAssertions;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.PatternMatchingAlgorithmHelpersTests;

[TestFixture]
public class GenerateNumbersFromPatternTests
{
    [Test]
    public void Given_Empty_Pattern_When_GenerateNumbersFromPattern_Method_Is_Invoked_Should_Return_Empty()
    {
        // Arrange
        var rng = new Random(1);

        // Act
        var result = PatternMatchingAlgorithmHelpers.GenerateNumbersFromPattern("", 10, rng);

        // Assert
        result.IsEmpty.Should().BeTrue();
    }

    [Test]
    public void Given_Tokens_When_GenerateNumbersFromPattern_Method_Is_Invoked_Should_Respect_OddEven_And_HighLow_Constraints()
    {
        // Arrange
        var rng = new Random(2);
        const int maxRange = 10; // half = 5
        const string pattern = "OL,EH,OH,EL";

        // Act
        var result = PatternMatchingAlgorithmHelpers.GenerateNumbersFromPattern(pattern, maxRange, rng);

        // Assert (counts per constraint family)
        var tokens = pattern.Split(',');
        const int half = maxRange / 2;

        var needOdd = tokens.Count(t => t.Contains('O'));
        var needEven = tokens.Count(t => t.Contains('E'));
        var needLow = tokens.Count(t => t.Contains('L'));
        var needHigh = tokens.Count(t => t.Contains('H'));

        ((result.Count(n => (n & 1) == 1) == needOdd) &&
         (result.Count(n => (n & 1) == 0) == needEven) &&
         (result.Count(n => n <= half) == needLow) &&
         (result.Count(n => n > half) == needHigh)).Should().BeTrue();
    }

    [Test]
    public void Given_Exhausted_Token_Pool_When_GenerateNumbersFromPattern_Method_Is_Invoked_Should_Use_Fallback_To_Fill_Count()
    {
        // Arrange
        var rng = new Random(3);

        // Act
        var result = PatternMatchingAlgorithmHelpers.GenerateNumbersFromPattern("OL,OL", 2, rng);

        // Assert
        result.Length.Should().Be(2);
    }
}