using System.Collections.Immutable;
using FluentAssertions;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.CyclicPatternsAlgorithmHelpersTests;

[TestFixture]
public class GenerateRandomNumbersTests
{
    [Test]
    public void Given_Count_Less_Or_Equal_Zero_When_GenerateRandomNumbers_Method_Is_Invoked_Should_Return_Empty()
    {
        // Arrange
        var rng = new Random(111);

        // Act
        var result = CyclicPatternsAlgorithmHelpers.GenerateRandomNumbers(1, 10, ImmutableArray<int>.Empty, 0, rng);

        // Assert
        result.IsEmpty.Should().BeTrue();
    }

    [Test]
    public void Given_Exclusions_When_GenerateRandomNumbers_Method_Is_Invoked_Should_Not_Contain_Excluded()
    {
        // Arrange
        var rng = new Random(222);
        var exclude = ImmutableArray.Create(3, 4, 5);

        // Act
        var result = CyclicPatternsAlgorithmHelpers.GenerateRandomNumbers(1, 10, exclude, 6, rng);

        // Assert
        result.Intersect(exclude).Any().Should().BeFalse();
    }

    [Test]
    public void Given_Range_When_GenerateRandomNumbers_Method_Is_Invoked_Should_Return_Values_Within_Range()
    {
        // Arrange
        var rng = new Random(333);

        // Act
        var result = CyclicPatternsAlgorithmHelpers.GenerateRandomNumbers(5, 7, ImmutableArray<int>.Empty, 2, rng);

        // Assert
        result.All(n => n >= 5 && n <= 7).Should().BeTrue();
    }

    [Test]
    public void Given_Sufficient_Candidates_When_GenerateRandomNumbers_Method_Is_Invoked_Should_Return_Requested_Count()
    {
        // Arrange
        var rng = new Random(444);

        // Act
        var result = CyclicPatternsAlgorithmHelpers.GenerateRandomNumbers(1, 20, ImmutableArray<int>.Empty, 8, rng);

        // Assert
        result.Length.Should().Be(8);
    }
}