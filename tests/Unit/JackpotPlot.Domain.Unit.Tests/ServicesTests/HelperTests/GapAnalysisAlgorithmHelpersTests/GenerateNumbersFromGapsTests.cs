using FluentAssertions;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.ServicesTests.HelperTests.GapAnalysisAlgorithmHelpersTests;

[TestFixture]
public class GenerateNumbersFromGapsTests
{
    [Test]
    public void Given_Non_Positive_Target_When_GenerateNumbersFromGaps_Method_Is_Invoked_Should_Return_Empty()
    {
        // Arrange
        var rng = new Random(1);

        // Act
        var result = GapAnalysisAlgorithmHelpers.GenerateNumbersFromGaps(30, new List<int> { 2, 3 }, 0, rng);

        // Assert
        result.IsEmpty.Should().BeTrue();
    }

    [Test]
    public void Given_Gaps_Within_Range_When_GenerateNumbersFromGaps_Method_Is_Invoked_Should_Return_Ascending_In_Range()
    {
        // Arrange
        var rng = new Random(2);
        var gaps = new List<int> { 2, 3, 4 };

        // Act
        var result = GapAnalysisAlgorithmHelpers.GenerateNumbersFromGaps(50, gaps, 4, rng);

        // Assert
        (result.SequenceEqual(result.OrderBy(x => x)) && result.All(n => n >= 1 && n <= 50)).Should().BeTrue();
    }

    [Test]
    public void Given_Gaps_That_Overflow_Range_When_GenerateNumbersFromGaps_Method_Is_Invoked_Should_Fill_To_Target_Count()
    {
        // Arrange
        var rng = new Random(3);
        var gaps = new List<int> { 15, 20, 25 }; // likely to exceed small range from low start

        // Act
        var result = GapAnalysisAlgorithmHelpers.GenerateNumbersFromGaps(25, gaps, 5, rng);

        // Assert
        result.Length.Should().Be(5);
    }
}