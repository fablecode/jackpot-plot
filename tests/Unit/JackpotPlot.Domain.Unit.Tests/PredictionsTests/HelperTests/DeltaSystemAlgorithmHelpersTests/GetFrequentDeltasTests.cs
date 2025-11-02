using FluentAssertions;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.DeltaSystemAlgorithmHelpersTests;

[TestFixture]
public class GetFrequentDeltasTests
{
    [Test]
    public void Given_Take_Zero_When_GetFrequentDeltas_Method_Is_Invoked_Should_Return_Empty()
    {
        // Arrange
        var deltas = new List<int> { 1, 1, 2 };

        // Act
        var result = DeltaSystemAlgorithmHelpers.GetFrequentDeltas(deltas, 0);

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public void Given_Deltas_When_GetFrequentDeltas_Method_Is_Invoked_Should_Order_By_Frequency_Then_By_Value()
    {
        // Arrange
        // freq: 3→3, 1→2, 2→2 (tie between 1 and 2; thenBy key -> 1 before 2)
        var deltas = new List<int> { 3, 3, 3, 1, 1, 2, 2 };

        // Act
        var result = DeltaSystemAlgorithmHelpers.GetFrequentDeltas(deltas, 3);

        // Assert
        result.Should().ContainInOrder(3, 1, 2);
    }

    [Test]
    public void Given_More_Available_Than_Requested_When_GetFrequentDeltas_Method_Is_Invoked_Should_Truncate()
    {
        // Arrange
        var deltas = new List<int> { 1, 1, 1, 2, 2, 3, 4, 5 };

        // Act
        var result = DeltaSystemAlgorithmHelpers.GetFrequentDeltas(deltas, 2);

        // Assert
        result.Should().HaveCount(2);
    }
}