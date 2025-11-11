using FluentAssertions;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.SeasonalPatternsAlgorithmHelpersTests;

[TestFixture]
public class GenerateFromSeasonalFrequenciesTests
{
    [Test]
    public void Given_Zero_Take_When_GenerateFromSeasonalFrequencies_Is_Invoked_Should_Return_Empty()
    {
        // Arrange
        var seasonal = new Dictionary<int, int> { { 1, 5 }, { 2, 3 } };
        var rng = new Random(42);

        // Act
        var result = SeasonalPatternsAlgorithmHelpers.GenerateFromSeasonalFrequencies(seasonal, take: 0, rng);

        // Assert
        result.IsEmpty.Should().BeTrue();
    }

    [Test]
    public void Given_Frequencies_When_GenerateFromSeasonalFrequencies_Is_Invoked_Should_Return_Top_By_Frequency()
    {
        // Arrange
        var seasonal = new Dictionary<int, int> { { 7, 10 }, { 2, 3 }, { 9, 8 } };
        var rng = new Random(42);

        // Act
        var result = SeasonalPatternsAlgorithmHelpers.GenerateFromSeasonalFrequencies(seasonal, take: 1, rng);

        // Assert
        result.Single().Should().Be(7);
    }
}