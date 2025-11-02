using FluentAssertions;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.InvertedFrequencyAlgorithmHelpersTests;

[TestFixture]
public class GenerateFromInvertedFrequenciesTests
{
    [Test]
    public void Given_Non_Positive_Take_When_GenerateFromInvertedFrequencies_Method_Is_Invoked_Should_Return_Empty()
    {
        // Arrange
        var map = new Dictionary<int, int> { [1] = 0, [2] = 1 };
        var rng = new Random(1);

        // Act
        var result = InvertedFrequencyAlgorithmHelpers.GenerateFromInvertedFrequencies(map, take: 0, rng).ToList();

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public void Given_Ties_And_Take_Less_Than_Total_When_GenerateFromInvertedFrequencies_Method_Is_Invoked_Should_Not_Include_Hotter_Group()
    {
        // Arrange
        // Cold group (freq 0): {1,2}; Hotter group (freq 2): {3}
        var map = new Dictionary<int, int> { [1] = 0, [2] = 0, [3] = 2 };
        var rng = new Random(2);

        // Act
        var result = InvertedFrequencyAlgorithmHelpers.GenerateFromInvertedFrequencies(map, take: 2, rng).ToList();

        // Assert
        result.Contains(3).Should().BeFalse(); // must come entirely from coldest group
    }

    [Test]
    public void Given_Enough_Candidates_When_GenerateFromInvertedFrequencies_Method_Is_Invoked_Should_Return_Requested_Count()
    {
        // Arrange
        var map = new Dictionary<int, int> { [1] = 0, [2] = 0, [3] = 1, [4] = 1, [5] = 2 };
        var rng = new Random(3);

        // Act
        var result = InvertedFrequencyAlgorithmHelpers.GenerateFromInvertedFrequencies(map, take: 4, rng).ToList();

        // Assert
        result.Count.Should().Be(4);
    }
}