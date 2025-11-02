using FluentAssertions;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.GapAnalysisAlgorithmHelpersTests;

[TestFixture]
public class SelectGapsTests
{
    [Test]
    public void Given_Empty_Frequencies_When_SelectGaps_Method_Is_Invoked_Should_Return_Empty_List()
    {
        // Arrange
        var freq = new Dictionary<int, int>();

        // Act
        var result = GapAnalysisAlgorithmHelpers.SelectGaps(freq, count: 3);

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public void Given_Frequencies_When_SelectGaps_Method_Is_Invoked_Should_Order_By_Frequency_Then_Gap_Value()
    {
        // Arrange
        // gaps: 3→3 times, 1→2 times, 2→2 times (tie -> lower key first)
        var freq = new Dictionary<int, int> { [3] = 3, [1] = 2, [2] = 2, [4] = 1 };

        // Act
        var result = GapAnalysisAlgorithmHelpers.SelectGaps(freq, count: 3);

        // Assert
        result.Should().ContainInOrder(3, 1, 2);
    }
}