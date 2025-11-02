using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;
using System.Collections.Immutable;
using FluentAssertions;

namespace JackpotPlot.Domain.Unit.Tests.ServicesTests.HelperTests.GroupSelectionAlgorithmHelpersTests;

[TestFixture]
public class CalculateGroupConfidenceTests
{
    [Test]
    public void Given_Empty_Predicted_When_CalculateGroupConfidence_Method_Is_Invoked_Should_Return_Zero()
    {
        // Arrange
        var groups = GroupSelectionAlgorithmHelpers.DivideIntoGroups(20, 4);
        var historicalGroupCounts = groups.ToDictionary(g => g, _ => 0);
        var predicted = ImmutableArray<int>.Empty;

        // Act
        var confidence = GroupSelectionAlgorithmHelpers.CalculateGroupConfidence(new List<HistoricalDraw>(), predicted, groups, historicalGroupCounts);

        // Assert
        confidence.Should().Be(0d);
    }

    [Test]
    public void Given_Matching_Distributions_When_CalculateGroupConfidence_Method_Is_Invoked_Should_Return_One()
    {
        // Arrange
        var groups = new List<(int start, int end)> { (1, 5), (6, 10) };
        var historicalCounts = new Dictionary<(int, int), int>
        {
            [(1, 5)] = 3,
            [(6, 10)] = 2
        };
        // Match the distribution: 3 numbers in [1..5], 2 numbers in [6..10]
        var predicted = ImmutableArray.Create(1, 2, 3, 6, 7);

        // Act
        var confidence = GroupSelectionAlgorithmHelpers.CalculateGroupConfidence(new List<HistoricalDraw>(), predicted, groups, historicalCounts);

        // Assert (L1 distance 0 ⇒ 1 / (1+0) = 1)
        confidence.Should().Be(1d);
    }

    [Test]
    public void Given_Different_Distributions_When_CalculateGroupConfidence_Method_Is_Invoked_Should_Be_Less_Than_One()
    {
        // Arrange
        var groups = new List<(int start, int end)> { (1, 5), (6, 10) };
        var historicalCounts = new Dictionary<(int, int), int>
        {
            [(1, 5)] = 5,
            [(6, 10)] = 0
        };
        var predicted = ImmutableArray.Create(6, 7, 8, 9, 10); // all in second group

        // Act
        var confidence = GroupSelectionAlgorithmHelpers.CalculateGroupConfidence(new List<HistoricalDraw>(), predicted, groups, historicalCounts);

        // Assert
        (confidence < 1d).Should().BeTrue();
    }
}