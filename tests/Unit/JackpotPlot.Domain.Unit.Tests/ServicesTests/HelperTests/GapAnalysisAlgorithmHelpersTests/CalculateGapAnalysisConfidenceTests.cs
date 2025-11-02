using FluentAssertions;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.ServicesTests.HelperTests.GapAnalysisAlgorithmHelpersTests;

[TestFixture]
public class CalculateGapAnalysisConfidenceTests
{
    [Test]
    public void Given_Empty_History_Or_Too_Few_Predicted_When_CalculateGapAnalysisConfidence_Method_Is_Invoked_Should_Return_Zero()
    {
        // Arrange
        var history = new List<HistoricalDraw> { AlgorithmsTestHelperTests.Draw(1, 2, 3) };
        var predicted = new List<int> { 5 }; // <2 numbers

        // Act
        var confidence = GapAnalysisAlgorithmHelpers.CalculateGapAnalysisConfidence(history, predicted);

        // Assert
        confidence.Should().Be(0.0);
    }

    [Test]
    public void Given_Matching_Predicted_Gaps_When_CalculateGapAnalysisConfidence_Method_Is_Invoked_Should_Return_Expected_Ratio()
    {
        // Arrange
        // draw1 gaps: [2,3,4]; draw2 gaps: [4]; total = 4
        var history = new List<HistoricalDraw> { AlgorithmsTestHelperTests.Draw(1, 3, 6, 10), AlgorithmsTestHelperTests.Draw(5, 9) };
        // predicted gaps: [2,3] (from 10,12,15)
        var predicted = new List<int> { 10, 12, 15 };

        // Act
        var confidence = GapAnalysisAlgorithmHelpers.CalculateGapAnalysisConfidence(history, predicted);

        // Assert
        confidence.Should().BeApproximately(0.5, 1e-9);
    }
}