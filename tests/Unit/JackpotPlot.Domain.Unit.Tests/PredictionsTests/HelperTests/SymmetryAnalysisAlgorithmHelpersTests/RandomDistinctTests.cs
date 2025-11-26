using System.Collections.Immutable;
using FluentAssertions;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.SymmetryAnalysisAlgorithmHelpersTests;

[TestFixture]
public class RandomDistinctTests
{
    [Test]
    public void Given_Non_Positive_Count_When_RandomDistinct_Is_Invoked_Should_Return_Empty()
    {
        // Arrange
        var rng = new Random(6);

        // Act
        var result = SymmetryAnalysisAlgorithmHelpers.RandomDistinct(1, 10, ImmutableArray<int>.Empty, 0, rng);

        // Assert
        result.IsEmpty.Should().BeTrue();
    }

    [Test]
    public void Given_Exclude_Set_When_RandomDistinct_Is_Invoked_Should_Not_Contain_Excluded()
    {
        // Arrange
        var exclude = ImmutableArray.Create(3, 4, 5);
        var rng = new Random(7);

        // Act
        var result = SymmetryAnalysisAlgorithmHelpers.RandomDistinct(1, 10, exclude, 5, rng);

        // Assert
        result.Should().OnlyContain(n => n >= 1 && n <= 10 && !exclude.Contains(n));
    }

    [Test]
    public void Given_Valid_Range_When_RandomDistinct_Is_Invoked_Should_Return_Unique_Items()
    {
        // Arrange
        var rng = new Random(8);

        // Act
        var result = SymmetryAnalysisAlgorithmHelpers.RandomDistinct(5, 15, ImmutableArray<int>.Empty, 6, rng);

        // Assert
        result.Should().OnlyHaveUniqueItems();
    }
}