using System.Collections.Immutable;
using FluentAssertions;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.GroupSelectionAlgorithmHelpersTests;

[TestFixture]
public class RandomDistinctTests
{
    [Test]
    public void Given_Non_Positive_Count_When_RandomDistinct_Method_Is_Invoked_Should_Return_Empty()
    {
        // Arrange
        var rng = new Random(4);

        // Act
        var result = GroupSelectionAlgorithmHelpers.RandomDistinct(1, 10, ImmutableArray<int>.Empty, 0, rng);

        // Assert
        result.IsEmpty.Should().BeTrue();
    }

    [Test]
    public void Given_Exclusions_When_RandomDistinct_Method_Is_Invoked_Should_Not_Contain_Excluded()
    {
        // Arrange
        var rng = new Random(5);
        var exclude = ImmutableArray.Create(2, 4, 6);

        // Act
        var result = GroupSelectionAlgorithmHelpers.RandomDistinct(1, 10, exclude, 5, rng);

        // Assert
        result.Intersect(exclude).Any().Should().BeFalse();
    }

    [Test]
    public void Given_Count_Exceeding_Candidates_When_RandomDistinct_Method_Is_Invoked_Should_Return_All_Candidates()
    {
        // Arrange
        var rng = new Random(6);
        var exclude = ImmutableArray.Create(1, 2, 3, 4, 5, 6, 7, 8); // leaves 9,10

        // Act
        var result = GroupSelectionAlgorithmHelpers.RandomDistinct(1, 10, exclude, 5, rng);

        // Assert
        result.Length.Should().Be(2);
    }

    [Test]
    public void Given_Range_When_RandomDistinct_Method_Is_Invoked_Should_Stay_Within_Range()
    {
        // Arrange
        var rng = new Random(7);

        // Act
        var result = GroupSelectionAlgorithmHelpers.RandomDistinct(10, 20, ImmutableArray<int>.Empty, 6, rng);

        // Assert
        result.All(n => n >= 10 && n <= 20).Should().BeTrue();
    }
}