using FluentAssertions;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.InvertedFrequencyAlgorithmHelpersTests;

[TestFixture]
public class AnalyzeInvertedFrequenciesTests
{
    [Test]
    public void Given_No_Draws_When_AnalyzeInvertedFrequencies_Method_Is_Invoked_Should_Return_Map_For_Entire_Range()
    {
        // Arrange
        var draws = Array.Empty<HistoricalDraw>();

        // Act
        var result = InvertedFrequencyAlgorithmHelpers.AnalyzeInvertedFrequencies(draws, numberRange: 10);

        // Assert
        result.Count.Should().Be(10);
    }

    [Test]
    public void Given_Mixed_Counts_When_AnalyzeInvertedFrequencies_Method_Is_Invoked_Should_Place_Coldest_Number_First()
    {
        // Arrange
        // number 1 appears 0 times, 2 once, 3 twice → coldest (0) should lead after sorting asc by freq
        var draws = new[]
        {
            AlgorithmsTestHelperTests.Draw(2, 3),
            AlgorithmsTestHelperTests.Draw(3)
        };

        // Act
        var result = InvertedFrequencyAlgorithmHelpers.AnalyzeInvertedFrequencies(draws, numberRange: 5);

        // Assert
        result.First().Key.Should().Be(1); // coldest first
    }

    [Test]
    public void Given_OutOfRange_Numbers_When_AnalyzeInvertedFrequencies_Method_Is_Invoked_Should_Ignore_Them()
    {
        // Arrange
        var draws = new[] { AlgorithmsTestHelperTests.Draw(-1, 0, 1, 15) };

        // Act
        var result = InvertedFrequencyAlgorithmHelpers.AnalyzeInvertedFrequencies(draws, numberRange: 10);

        // Assert
        result[1].Should().Be(1); // only '1' counted
    }
}