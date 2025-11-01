using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;
using System.Collections.Immutable;
using FluentAssertions;

namespace JackpotPlot.Domain.Unit.Tests.ServicesTests.HelperTests.DrawPositionAnalysisAlgorithmHelpersTests;

[TestFixture]
public class GenerateRandomNumbersTests
{
    [Test]
    public void Given_Non_Positive_Count_When_GenerateRandomNumbers_Method_Is_Invoked_Should_Return_Empty()
    {
        // Arrange
        var rng = new Random(1);

        // Act
        var result = DrawPositionAnalysisAlgorithmHelpers.GenerateRandomNumbers(1, 10, ImmutableArray<int>.Empty, 0, rng);

        // Assert
        result.IsEmpty.Should().BeTrue();
    }

    [Test]
    public void Given_Exclusions_When_GenerateRandomNumbers_Method_Is_Invoked_Should_Not_Return_Excluded()
    {
        // Arrange
        var rng = new Random(2);
        var exclude = ImmutableArray.Create(3, 4, 5);

        // Act
        var result = DrawPositionAnalysisAlgorithmHelpers.GenerateRandomNumbers(1, 10, exclude, 5, rng);

        // Assert
        result.Intersect(exclude).Any().Should().BeFalse();
    }

    [Test]
    public void Given_Range_When_GenerateRandomNumbers_Method_Is_Invoked_Should_Return_Values_Within_Range()
    {
        // Arrange
        var rng = new Random(3);

        // Act
        var result = DrawPositionAnalysisAlgorithmHelpers.GenerateRandomNumbers(5, 7, ImmutableArray<int>.Empty, 2, rng);

        // Assert
        result.All(n => n >= 5 && n <= 7).Should().BeTrue();
    }

    [Test]
    public void Given_Sufficient_Candidates_When_GenerateRandomNumbers_Method_Is_Invoked_Should_Return_Requested_Count()
    {
        // Arrange
        var rng = new Random(4);

        // Act
        var result = DrawPositionAnalysisAlgorithmHelpers.GenerateRandomNumbers(1, 20, ImmutableArray<int>.Empty, 6, rng);

        // Assert
        result.Length.Should().Be(6);
    }
}