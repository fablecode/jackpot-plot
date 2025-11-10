using FluentAssertions;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.PatternMatchingAlgorithmHelpersTests;

[TestFixture]
public class SelectMostFrequentPatternTests
{
    [Test]
    public void Given_Pattern_Map_When_SelectMostFrequentPattern_Method_Is_Invoked_Should_Return_Key_With_Max_Count()
    {
        // Arrange
        var patterns = new Dictionary<string, int>(StringComparer.Ordinal)
        {
            ["A"] = 1,
            ["B"] = 5,
            ["C"] = 3
        };

        // Act
        var key = PatternMatchingAlgorithmHelpers.SelectMostFrequentPattern(patterns);

        // Assert
        key.Should().Be("B");
    }

    [Test]
    public void Given_Empty_Map_When_SelectMostFrequentPattern_Method_Is_Invoked_Should_Return_Empty_String()
    {
        // Arrange
        var patterns = new Dictionary<string, int>(StringComparer.Ordinal);

        // Act
        var key = PatternMatchingAlgorithmHelpers.SelectMostFrequentPattern(patterns);

        // Assert
        key.Should().BeEmpty();
    }
}