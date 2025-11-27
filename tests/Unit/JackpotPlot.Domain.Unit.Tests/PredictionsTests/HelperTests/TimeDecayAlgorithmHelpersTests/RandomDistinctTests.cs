using System.Collections.Immutable;
using FluentAssertions;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.TimeDecayAlgorithmHelpersTests;

[TestFixture]
public class RandomDistinctTests
{
    [Test]
    public void Given_Non_Positive_Count_When_RandomDistinct_Is_Invoked_Should_Return_Empty()
    {
        // Arrange
        var rng = new Random(4);

        // Act
        var result = TimeDecayAlgorithmHelpers.RandomDistinct(1, 10, ImmutableArray<int>.Empty, 0, rng);

        // Assert
        result.IsEmpty.Should().BeTrue();
    }

    [Test]
    public void Given_Exclude_Set_When_RandomDistinct_Is_Invoked_Should_Not_Return_Excluded()
    {
        // Arrange
        var exclude = ImmutableArray.Create(3, 4, 5);
        var rng = new Random(5);

        // Act
        var result = TimeDecayAlgorithmHelpers.RandomDistinct(1, 10, exclude, 5, rng);

        // Assert
        (result.All(n => n >= 1 && n <= 10 && !exclude.Contains(n))).Should().BeTrue();
    }

    [Test]
    public void Given_Valid_Range_When_RandomDistinct_Is_Invoked_Should_Return_Distinct_Within_Range_And_Count()
    {
        // Arrange
        var rng = new Random(6);

        // Act
        var result = TimeDecayAlgorithmHelpers.RandomDistinct(5, 15, ImmutableArray<int>.Empty, 6, rng);

        // Assert
        (result.Length == 6
         && result.Distinct().Count() == 6
         && result.All(n => n >= 5 && n <= 15)).Should().BeTrue();
    }
}