using System.Collections.Immutable;
using FluentAssertions;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.SkewnessAnalysisAlgorithmHelpersTests;

[TestFixture]
public class RandomDistinctTests
{
    [Test]
    public void Given_Non_Positive_Count_When_RandomDistinct_Is_Invoked_Should_Return_Empty()
    {
        // Arrange
        var rng = new Random(5);

        // Act
        var result = SkewnessAnalysisAlgorithmHelpers.RandomDistinct(1, 10, ImmutableArray<int>.Empty, 0, rng);

        // Assert
        result.IsEmpty.Should().BeTrue();
    }

    [Test]
    public void Given_Exclude_Set_When_RandomDistinct_Is_Invoked_Should_Not_Return_Excluded()
    {
        // Arrange
        var exclude = ImmutableArray.Create(3, 4, 5);
        var rng = new Random(8);

        // Act
        var result = SkewnessAnalysisAlgorithmHelpers.RandomDistinct(1, 10, exclude, 5, rng);

        // Assert
        result.Should().OnlyContain(n => n >= 1 && n <= 10 && !exclude.Contains(n));
    }

    [Test]
    public void Given_Valid_Params_When_RandomDistinct_Is_Invoked_Should_Return_Requested_Count()
    {
        // Arrange
        var rng = new Random(13);

        // Act
        var result = SkewnessAnalysisAlgorithmHelpers.RandomDistinct(5, 15, ImmutableArray<int>.Empty, 6, rng);

        // Assert
        result.Length.Should().Be(6);
    }
}