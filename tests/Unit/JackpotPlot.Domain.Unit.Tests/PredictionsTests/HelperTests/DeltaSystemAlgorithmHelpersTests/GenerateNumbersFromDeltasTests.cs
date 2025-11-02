using FluentAssertions;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.DeltaSystemAlgorithmHelpersTests;

[TestFixture]
public class GenerateNumbersFromDeltasTests
{
    [Test]
    public void Given_Non_Positive_Target_When_GenerateNumbersFromDeltas_Method_Is_Invoked_Should_Return_Empty()
    {
        // Arrange
        var rng = new Random(1);

        // Act
        var result = DeltaSystemAlgorithmHelpers.GenerateNumbersFromDeltas(new List<int> { 1, 2 }, 30, 0, rng);

        // Assert
        result.IsEmpty.Should().BeTrue();
    }

    [Test]
    public void Given_Deltas_When_GenerateNumbersFromDeltas_Method_Is_Invoked_Should_Produce_Ascending_Distinct_Within_Range()
    {
        // Arrange
        var rng = new Random(2);
        var deltas = new List<int> { 2, 3, 4 };

        // Act
        var result = DeltaSystemAlgorithmHelpers.GenerateNumbersFromDeltas(deltas, 50, 4, rng);

        // Assert
        (result.SequenceEqual(result.OrderBy(x => x)) && result.Distinct().Count() == result.Length
         && result.All(n => n >= 1 && n <= 50)).Should().BeTrue();
    }

    [Test]
    public void Given_Deltas_That_Overflow_When_GenerateNumbersFromDeltas_Method_Is_Invoked_Should_Fill_To_Target_Count()
    {
        // Arrange
        var rng = new Random(3);
        var deltas = new List<int> { 15, 20, 25 }; // likely to overflow quickly with small range

        // Act
        var result = DeltaSystemAlgorithmHelpers.GenerateNumbersFromDeltas(deltas, 25, 5, rng);

        // Assert
        result.Length.Should().Be(5);
    }

    [Test]
    public void Given_Repeated_Deltas_When_GenerateNumbersFromDeltas_Method_Is_Invoked_Should_Avoid_Duplicates()
    {
        // Arrange
        var rng = new Random(4);
        var deltas = new List<int> { 1, 1, 1, 1, 1, 1 };

        // Act
        var result = DeltaSystemAlgorithmHelpers.GenerateNumbersFromDeltas(deltas, 10, 6, rng);

        // Assert
        result.Distinct().Count().Should().Be(result.Length);
    }
}