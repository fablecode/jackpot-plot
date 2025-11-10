using System.Collections.Immutable;
using FluentAssertions;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.QuadrantAnalysisAlgorithmHelpersTests;

[TestFixture]
public class RandomDistinctTests
{
    [Test]
    public void Given_Non_Positive_Count_When_RandomDistinct_Is_Called_Should_Return_Empty()
    {
        // Arrange
        var rng = new Random(6);

        // Act
        var result = QuadrantAnalysisAlgorithmHelpers.RandomDistinct(1, 10, ImmutableArray<int>.Empty, 0, rng);

        // Assert
        result.IsEmpty.Should().BeTrue();
    }

    [Test]
    public void Given_Exclusions_When_RandomDistinct_Is_Called_Should_Not_Return_Excluded()
    {
        // Arrange
        var rng = new Random(7);
        var exclude = ImmutableArray.Create(3, 4, 5);

        // Act
        var result = QuadrantAnalysisAlgorithmHelpers.RandomDistinct(1, 10, exclude, 5, rng);

        // Assert
        result.Intersect(exclude).Any().Should().BeFalse();
    }

    [Test]
    public void Given_Range_When_RandomDistinct_Is_Called_Should_Return_In_Range_And_Distinct()
    {
        // Arrange
        var rng = new Random(8);

        // Act
        var result = QuadrantAnalysisAlgorithmHelpers.RandomDistinct(5, 7, ImmutableArray<int>.Empty, 2, rng);

        // Assert
        (result.All(n => n is >= 5 and <= 7) && result.Distinct().Count() == result.Length).Should().BeTrue();
    }
}