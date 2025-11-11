using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;
using System.Collections.Immutable;
using FluentAssertions;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.SeasonalPatternsAlgorithmHelpersTests;

[TestFixture]
public class GetSeasonTests
{
    [Test]
    public void Given_Date_In_January_When_GetSeason_Is_Invoked_Should_Return_Winter()
    {
        // Arrange
        var date = new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Utc);

        // Act
        var season = SeasonalPatternsAlgorithmHelpers.GetSeason(date);

        // Assert
        season.Should().Be("Winter");
    }

    [Test]
    public void Given_Date_In_April_When_GetSeason_Is_Invoked_Should_Return_Spring()
    {
        // Arrange
        var date = new DateTime(2025, 4, 2, 0, 0, 0, DateTimeKind.Utc);

        // Act
        var season = SeasonalPatternsAlgorithmHelpers.GetSeason(date);

        // Assert
        season.Should().Be("Spring");
    }

    [Test]
    public void Given_Date_In_July_When_GetSeason_Is_Invoked_Should_Return_Summer()
    {
        // Arrange
        var date = new DateTime(2025, 7, 10, 0, 0, 0, DateTimeKind.Utc);

        // Act
        var season = SeasonalPatternsAlgorithmHelpers.GetSeason(date);

        // Assert
        season.Should().Be("Summer");
    }

    [Test]
    public void Given_Date_In_October_When_GetSeason_Is_Invoked_Should_Return_Fall()
    {
        // Arrange
        var date = new DateTime(2025, 10, 31, 0, 0, 0, DateTimeKind.Utc);

        // Act
        var season = SeasonalPatternsAlgorithmHelpers.GetSeason(date);

        // Assert
        season.Should().Be("Fall");
    }

    [Test]
    public void Given_Non_Positive_Count_When_RandomDistinct_Is_Invoked_Should_Return_Empty()
    {
        // Arrange
        var rng = new Random(4);

        // Act
        var result = SeasonalPatternsAlgorithmHelpers.RandomDistinct(1, 10, ImmutableArray<int>.Empty, 0, rng);

        // Assert
        result.IsEmpty.Should().BeTrue();
    }

    [Test]
    public void Given_Exclude_Set_When_RandomDistinct_Is_Invoked_Should_Not_Return_Excluded()
    {
        // Arrange
        var exclude = ImmutableArray.Create(3, 4, 5);
        var rng = new Random(7);

        // Act
        var result = SeasonalPatternsAlgorithmHelpers.RandomDistinct(1, 10, exclude, 5, rng);

        // Assert
        result.Should().OnlyContain(n => n >= 1 && n <= 10 && !exclude.Contains(n));
    }

    [Test]
    public void Given_Valid_Range_When_RandomDistinct_Is_Invoked_Should_Return_Distinct_Items()
    {
        // Arrange
        var rng = new Random(123);

        // Act
        var result = SeasonalPatternsAlgorithmHelpers.RandomDistinct(5, 15, ImmutableArray<int>.Empty, 6, rng);

        // Assert
        result.Should().OnlyHaveUniqueItems();
    }
}