using FluentAssertions;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.DeltaSystemAlgorithmHelpersTests;

[TestFixture]
public class CalculateDeltasTests
{
    [Test]
    public void Given_No_History_When_CalculateDeltas_Method_Is_Invoked_Should_Return_Empty()
    {
        // Arrange
        // Act
        var result = DeltaSystemAlgorithmHelpers.CalculateDeltas(new List<HistoricalDraw>());

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public void Given_Draws_When_CalculateDeltas_Method_Is_Invoked_Should_Return_All_Consecutive_Differences()
    {
        // Arrange
        var history = new List<HistoricalDraw>
        {
            AlgorithmsTestHelperTests.Draw(6,1,3,10),   // sorted -> 1,3,6,10 => deltas 2,3,4
            AlgorithmsTestHelperTests.Draw(20,21)       // delta 1
        };

        // Act
        var result = DeltaSystemAlgorithmHelpers.CalculateDeltas(history);

        // Assert
        result.Should().BeEquivalentTo(new List<int> { 2, 3, 4, 1 });
    }

    [Test]
    public void Given_Unsorted_List_When_CalculateDeltas_List_Is_Invoked_Should_Sort_Then_Diff()
    {
        // Arrange
        var numbers = new List<int> { 6, 1, 3, 10 };

        // Act
        var result = DeltaSystemAlgorithmHelpers.CalculateDeltas(numbers);

        // Assert
        result.Should().BeEquivalentTo(new List<int> { 2, 3, 4 });
    }
}