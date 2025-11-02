using FluentAssertions;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.CyclicPatternsAlgorithmHelpersTests;

[TestFixture]
public class AnalyzeCyclicPatternsTests
{
    [Test]
    public void Given_No_Draws_When_AnalyzeCyclicPatterns_Method_Is_Invoked_Should_Return_Dictionary_With_All_Empty_Gaps()
    {
        // Arrange
        var draws = Array.Empty<HistoricalDraw>();
        const int range = 5;

        // Act
        var cycles = CyclicPatternsAlgorithmHelpers.AnalyzeCyclicPatterns(draws, range);

        // Assert
        cycles.Count.Should().Be(range);
    }

    [Test]
    public void Given_Repeated_Number_When_AnalyzeCyclicPatterns_Method_Is_Invoked_Should_Record_Correct_Gaps()
    {
        // Arrange
        var start = new DateTime(2024, 1, 1);
        var draws = new[]
        {
            AlgorithmsTestHelperTests.Draw(start.AddDays(0), 7),
            AlgorithmsTestHelperTests.Draw(start.AddDays(1), 1),
            AlgorithmsTestHelperTests.Draw(start.AddDays(2), 7),
            AlgorithmsTestHelperTests.Draw(start.AddDays(3), 2),
            AlgorithmsTestHelperTests.Draw(start.AddDays(5), 7)
        };

        // Act
        var cycles = CyclicPatternsAlgorithmHelpers.AnalyzeCyclicPatterns(draws, numberRange: 10);

        // Assert
        cycles[7].Should().BeEquivalentTo(new List<int> { 2, 2 }, options => options.WithStrictOrdering());
    }
}