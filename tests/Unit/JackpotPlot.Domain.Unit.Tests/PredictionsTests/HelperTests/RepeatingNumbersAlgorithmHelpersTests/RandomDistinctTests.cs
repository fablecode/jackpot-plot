using System.Collections.Immutable;
using FluentAssertions;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.RepeatingNumbersAlgorithmHelpersTests;

[TestFixture]
public class RandomDistinctTests
{
    [Test]
    public void Given_Non_Positive_Count_When_RandomDistinct_Is_Invoked_Should_Return_Empty()
    {
        // Arrange
        var rng = new Random(4);

        // Act
        var result = RepeatingNumbersAlgorithmHelpers.RandomDistinct(1, 10, ImmutableArray<int>.Empty, 0, rng);

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
        var result = RepeatingNumbersAlgorithmHelpers.RandomDistinct(1, 10, exclude, 4, rng);

        // Assert
        result.Should().OnlyContain(n => n >= 1 && n <= 10 && !exclude.Contains(n));
    }

    [Test]
    public void Given_Request_For_Unique_Count_When_RandomDistinct_Is_Invoked_Should_Return_Unique_Items()
    {
        // Arrange
        var rng = new Random(42);

        // Act
        var result = RepeatingNumbersAlgorithmHelpers.RandomDistinct(1, 20, ImmutableArray<int>.Empty, 6, rng);

        // Assert
        result.Should().OnlyHaveUniqueItems();
    }

    [Test]
    public void Given_Valid_Range_And_Count_When_RandomDistinct_Is_Invoked_Should_Return_Requested_Count()
    {
        // Arrange
        var rng = new Random(123);

        // Act
        var result = RepeatingNumbersAlgorithmHelpers.RandomDistinct(5, 15, ImmutableArray<int>.Empty, 5, rng);

        // Assert
        result.Length.Should().Be(5);
    }
}