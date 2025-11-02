using System.Collections.Immutable;
using FluentAssertions;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.CyclicPatternsAlgorithmHelpersTests;

[TestFixture]
public class CalculateCyclicConfidenceTests
{
    [Test]
    public void Given_Empty_Predicted_When_CalculateCyclicConfidence_Method_Is_Invoked_Should_Return_Zero()
    {
        // Arrange
        var draws = Array.Empty<HistoricalDraw>();
        var predicted = ImmutableArray<int>.Empty;
        var cycles = new Dictionary<int, List<int>>();

        // Act
        var confidence = CyclicPatternsAlgorithmHelpers.CalculateCyclicConfidence(draws, predicted, cycles);

        // Assert
        confidence.Should().Be(0.0);
    }

    [Test]
    public void Given_Due_Number_When_CalculateCyclicConfidence_Method_Is_Invoked_Should_Return_One_For_Single_Predicted()
    {
        // Arrange
        // 7 appears at indices 0,1,2 → gaps [1,1], avg ~1; last index = 2; total draws = 5; distanceSinceLast = 2 ≥ avg
        var start = new DateTime(2024, 1, 1);
        var draws = new List<HistoricalDraw>
            {
                AlgorithmsTestHelperTests.Draw(start.AddDays(0), 7),
                AlgorithmsTestHelperTests.Draw(start.AddDays(1), 7),
                AlgorithmsTestHelperTests.Draw(start.AddDays(2), 7),
                AlgorithmsTestHelperTests.Draw(start.AddDays(3), 3),
                AlgorithmsTestHelperTests.Draw(start.AddDays(4), 10),
            };
        var cycles = CyclicPatternsAlgorithmHelpers.AnalyzeCyclicPatterns(draws, numberRange: 20);
        var predicted = ImmutableArray.Create(7);

        // Act
        var confidence = CyclicPatternsAlgorithmHelpers.CalculateCyclicConfidence(draws, predicted, cycles);

        // Assert
        confidence.Should().Be(1.0);
    }

    [Test]
    public void Given_Not_Due_Number_When_CalculateCyclicConfidence_Method_Is_Invoked_Should_Return_Zero_For_Single_Predicted()
    {
        // Arrange
        // 9 appears at indices 0,3 → gaps [3], avg = 3; last index = 3; total draws = 5; distanceSinceLast = 1 < avg
        var start = new DateTime(2024, 1, 1);
        var draws = new List<HistoricalDraw>
            {
                AlgorithmsTestHelperTests.Draw(start.AddDays(0), 9),
                AlgorithmsTestHelperTests.Draw(start.AddDays(1), 1),
                AlgorithmsTestHelperTests.Draw(start.AddDays(2), 2),
                AlgorithmsTestHelperTests.Draw(start.AddDays(3), 9),
                AlgorithmsTestHelperTests.Draw(start.AddDays(4), 5),
            };
        var cycles = CyclicPatternsAlgorithmHelpers.AnalyzeCyclicPatterns(draws, numberRange: 20);
        var predicted = ImmutableArray.Create(9);

        // Act
        var confidence = CyclicPatternsAlgorithmHelpers.CalculateCyclicConfidence(draws, predicted, cycles);

        // Assert
        confidence.Should().Be(0.0);
    }
}