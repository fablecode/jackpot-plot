using FluentAssertions;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.QuadrantAnalysisAlgorithmHelpersTests;

[TestFixture]
public class GenerateNumbersFromQuadrantsTests
{
    [Test]
    public void Given_Non_Positive_Count_When_GenerateNumbersFromQuadrants_Is_Called_Should_Return_Empty()
    {
        // Arrange
        var quads = new List<(int start, int end)> { (1, 5), (6, 10) };
        var freq = quads.ToDictionary(q => q, _ => 0);
        var rng = new Random(1);

        // Act
        var result = QuadrantAnalysisAlgorithmHelpers.GenerateNumbersFromQuadrants(quads, freq, 0, rng);

        // Assert
        result.IsEmpty.Should().BeTrue();
    }

    [Test]
    public void Given_No_History_When_GenerateNumbersFromQuadrants_Is_Called_Should_Return_Requested_Count()
    {
        // Arrange
        var quads = QuadrantAnalysisAlgorithmHelpers.DivideIntoQuadrants(12, 3);
        var freq = quads.ToDictionary(q => q, _ => 0);
        var rng = new Random(2);

        // Act
        var result = QuadrantAnalysisAlgorithmHelpers.GenerateNumbersFromQuadrants(quads, freq, count: 5, rng);

        // Assert
        result.Length.Should().Be(5);
    }

    [Test]
    public void Given_Frequencies_When_GenerateNumbersFromQuadrants_Is_Called_Should_Normalize_To_Requested_Count()
    {
        // Arrange
        var quads = QuadrantAnalysisAlgorithmHelpers.DivideIntoQuadrants(30, 3);
        var freq = quads.ToDictionary(q => q, _ => 1);
        var rng = new Random(3);

        // Act
        var result = QuadrantAnalysisAlgorithmHelpers.GenerateNumbersFromQuadrants(quads, freq, count: 2, rng);

        // Assert
        result.Length.Should().Be(2);
    }

    [Test]
    public void Given_Quadrants_When_GenerateNumbersFromQuadrants_Is_Called_Should_Return_In_Range_And_Distinct()
    {
        // Arrange
        var quads = QuadrantAnalysisAlgorithmHelpers.DivideIntoQuadrants(20, 4);
        var freq = new Dictionary<(int start, int end), int>
        {
            [quads[0]] = 5,
            [quads[1]] = 3,
            [quads[2]] = 1,
            [quads[3]] = 1
        };
        var rng = new Random(4);

        // Act
        var result = QuadrantAnalysisAlgorithmHelpers.GenerateNumbersFromQuadrants(quads, freq, count: 6, rng);

        // Assert
        (result.All(n => n >= 1 && n <= 20) && result.Distinct().Count() == result.Length).Should().BeTrue();
    }

    [Test]
    public void Given_Sparse_First_Quadrant_When_GenerateNumbersFromQuadrants_Is_Called_Should_Fill_Shortfall()
    {
        // Arrange
        var quads = new List<(int start, int end)> { (1, 1), (2, 3) };
        var freq = quads.ToDictionary(q => q, _ => 0);
        var rng = new Random(5);

        // Act
        var result = QuadrantAnalysisAlgorithmHelpers.GenerateNumbersFromQuadrants(quads, freq, count: 3, rng);

        // Assert
        result.Length.Should().Be(3);
    }
}