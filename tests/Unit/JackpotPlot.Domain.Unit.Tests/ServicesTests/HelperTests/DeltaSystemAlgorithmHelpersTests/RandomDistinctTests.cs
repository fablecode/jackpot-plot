using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;
using System.Collections.Immutable;
using FluentAssertions;

namespace JackpotPlot.Domain.Unit.Tests.ServicesTests.HelperTests.DeltaSystemAlgorithmHelpersTests;

[TestFixture]
public class RandomDistinctTests
{
    [Test]
    public void Given_Non_Positive_Count_When_RandomDistinct_Method_Is_Invoked_Should_Return_Empty()
    {
        // Arrange
        var rng = new Random(5);

        // Act
        var result = DeltaSystemAlgorithmHelpers.RandomDistinct(1, 10, ImmutableArray<int>.Empty, 0, rng);

        // Assert
        result.IsEmpty.Should().BeTrue();
    }

    [Test]
    public void Given_Exclusions_When_RandomDistinct_Method_Is_Invoked_Should_Not_Return_Excluded()
    {
        // Arrange
        var rng = new Random(6);
        var exclude = ImmutableArray.Create(3, 4, 5);

        // Act
        var result = DeltaSystemAlgorithmHelpers.RandomDistinct(1, 10, exclude, 5, rng);

        // Assert
        result.Intersect(exclude).Any().Should().BeFalse();
    }

    [Test]
    public void Given_Range_When_RandomDistinct_Method_Is_Invoked_Should_Return_In_Range_And_Distinct()
    {
        // Arrange
        var rng = new Random(7);

        // Act
        var result = DeltaSystemAlgorithmHelpers.RandomDistinct(5, 7, ImmutableArray<int>.Empty, 2, rng);

        // Assert
        (result.All(n => n >= 5 && n <= 7) && result.Distinct().Count() == result.Length).Should().BeTrue();
    }
}