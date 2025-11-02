using FluentAssertions;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.InvertedFrequencyAlgorithmHelpersTests;

[TestFixture]
public class InvertedFrequencyConfidenceTests
{
    [Test]
    public void Given_Empty_Predicted_When_InvertedFrequencyConfidence_Method_Is_Invoked_Should_Return_Zero()
    {
        // Arrange
        var draws = new[] { AlgorithmsTestHelperTests.Draw(1, 2, 3) };
        var predicted = new List<int>();

        // Act
        var confidence = InvertedFrequencyAlgorithmHelpers.InvertedFrequencyConfidence(draws, predicted);

        // Assert
        confidence.Should().Be(0.0);
    }

    [Test]
    public void Given_Mixed_Overlaps_When_InvertedFrequencyConfidence_Method_Is_Invoked_Should_Return_Expected_Ratio()
    {
        // Arrange
        // Intersections: [1,2,3]∩{2,4} = 1; [2,3,4]∩{2,4} = 2 → matches=3; denom=draws*predicted=2*2=4 → 0.75
        var draws = new[] { AlgorithmsTestHelperTests.Draw(1, 2, 3), AlgorithmsTestHelperTests.Draw(2, 3, 4) };
        var predicted = new List<int> { 2, 4 };

        // Act
        var confidence = InvertedFrequencyAlgorithmHelpers.InvertedFrequencyConfidence(draws, predicted);

        // Assert
        confidence.Should().BeApproximately(0.75, 1e-9);
    }
}