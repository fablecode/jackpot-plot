using System.Collections.Immutable;
using FluentAssertions;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.NumberSumAlgorithmHelpersTests;

public class RandomDistinctTests
{
    [Test]
    public void Given_Non_Positive_Count_When_RandomDistinct_Method_Is_Invoked_Should_Return_Empty()
    {
        // Arrange
        var rng = new Random(5);

        // Act
        var numbers = NumberSumAlgorithmHelpers.RandomDistinct(1, 10, ImmutableArray<int>.Empty, 0, rng);

        // Assert
        numbers.IsEmpty.Should().BeTrue();
    }

    [Test]
    public void Given_Exclusions_When_RandomDistinct_Method_Is_Invoked_Should_Not_Return_Excluded()
    {
        // Arrange
        var rng = new Random(6);
        var exclude = ImmutableArray.Create(3, 4, 5);

        // Act
        var numbers = NumberSumAlgorithmHelpers.RandomDistinct(1, 10, exclude, 5, rng);

        // Assert
        numbers.Intersect(exclude).Any().Should().BeFalse();
    }

    [Test]
    public void Given_Range_When_RandomDistinct_Method_Is_Invoked_Should_Return_In_Range_And_Distinct()
    {
        // Arrange
        var rng = new Random(7);

        // Act
        var numbers = NumberSumAlgorithmHelpers.RandomDistinct(5, 7, ImmutableArray<int>.Empty, 2, rng);

        // Assert
        (numbers.All(n => n is >= 5 and <= 7) && numbers.Distinct().Count() == numbers.Length).Should().BeTrue();
    }
}