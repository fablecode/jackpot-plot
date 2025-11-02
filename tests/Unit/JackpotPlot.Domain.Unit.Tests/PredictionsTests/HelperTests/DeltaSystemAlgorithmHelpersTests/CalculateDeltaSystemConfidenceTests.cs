using FluentAssertions;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.DeltaSystemAlgorithmHelpersTests;

[TestFixture]
public class CalculateDeltaSystemConfidenceTests
{
    [Test]
    public void Given_Empty_History_Or_Too_Few_Predicted_When_CalculateDeltaSystemConfidence_Method_Is_Method_Should_Return_Zero()
    {
        // Arrange
        var history = new List<HistoricalDraw> { AlgorithmsTestHelperTests.Draw(1, 2, 3) };
        var predicted = new List<int> { 5 }; // < 2 numbers

        // Act
        var confidence = DeltaSystemAlgorithmHelpers.CalculateDeltaSystemConfidence(history, predicted);

        // Assert
        confidence.Should().Be(0.0);
    }

    [Test]
    public void Given_Matching_Deltas_When_CalculateDeltaSystemConfidence_Method_Is_Invoked_Should_Return_Expected_Ratio()
    {
        // Arrange
        // history deltas: [2,3,4] and [1] => total = 4
        var history = new List<HistoricalDraw> { AlgorithmsTestHelperTests.Draw(1, 3, 6, 10), AlgorithmsTestHelperTests.Draw(7, 8) };
        // predicted deltas: [2,3] => intersect with first draw deltas has 2,3 => correct = 2
        var predicted = new List<int> { 10, 12, 15 };

        // Act
        var confidence = DeltaSystemAlgorithmHelpers.CalculateDeltaSystemConfidence(history, predicted);

        // Assert
        confidence.Should().BeApproximately(0.5, 1e-9);
    }
}