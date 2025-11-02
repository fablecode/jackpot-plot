using FluentAssertions;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.FrequencyAlgorithmHelpersTests;

[TestFixture]
public class FrequencyConfidenceTests
{
    [Test]
    public void Given_No_History_When_FrequencyConfidence_Method_Is_Invoked_Should_Return_Zero()
    {
        // Arrange
        var history = new List<HistoricalDraw>();
        var predicted = new List<int> { 1, 2, 3 };

        // Act
        var confidence = FrequencyAlgorithmHelpers.FrequencyConfidence(history, predicted);

        // Assert
        confidence.Should().Be(0.0);
    }

    [Test]
    public void Given_Empty_Predicted_When_FrequencyConfidence_Method_Is_Invoked_Should_Return_Zero()
    {
        // Arrange
        var history = new List<HistoricalDraw> { AlgorithmsTestHelperTests.Draw(1, 2, 3) };
        var predicted = new List<int>();

        // Act
        var confidence = FrequencyAlgorithmHelpers.FrequencyConfidence(history, predicted);

        // Assert
        confidence.Should().Be(0.0);
    }

    [Test]
    public void Given_No_Overlaps_When_FrequencyConfidence_Method_Is_Invoked_Should_Return_Zero()
    {
        // Arrange
        var history = new List<HistoricalDraw> { AlgorithmsTestHelperTests.Draw(1, 2, 3), AlgorithmsTestHelperTests.Draw(4, 5, 6) };
        var predicted = new List<int> { 7, 8 };

        // Act
        var confidence = FrequencyAlgorithmHelpers.FrequencyConfidence(history, predicted);

        // Assert
        confidence.Should().Be(0.0);
    }

    [Test]
    public void Given_Mixed_Overlaps_When_FrequencyConfidence_Method_Is_Invoked_Should_Return_Expected_Ratio()
    {
        // Arrange
        // Draw1 ∩ predicted = {3} -> 1; Draw2 ∩ predicted = {3,5} -> 2; total matches = 3
        // total positions considered = history.Count * predicted.Count = 2 * 2 = 4
        var history = new List<HistoricalDraw> { AlgorithmsTestHelperTests.Draw(1, 2, 3), AlgorithmsTestHelperTests.Draw(3, 4, 5) };
        var predicted = new List<int> { 3, 5 };

        // Act
        var confidence = FrequencyAlgorithmHelpers.FrequencyConfidence(history, predicted);

        // Assert
        confidence.Should().BeApproximately(0.75, 1e-9);
    }
}