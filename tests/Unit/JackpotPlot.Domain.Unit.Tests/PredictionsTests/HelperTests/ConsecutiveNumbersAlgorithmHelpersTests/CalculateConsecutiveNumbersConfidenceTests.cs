using FluentAssertions;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.ConsecutiveNumbersAlgorithmHelpersTests;

[TestFixture]
public class CalculateConsecutiveNumbersConfidenceTests
{
    [Test]
    public void Given_No_History_When_CalculateConsecutiveNumbersConfidence_Method_Is_Invoked_Should_Return_Zero()
    {
        // Arrange
        var draws = Array.Empty<HistoricalDraw>();
        var predicted = new List<int> { 1, 2, 3, 5 };

        // Act
        var result = ConsecutiveNumbersAlgorithmHelpers.CalculateConsecutiveNumbersConfidence(draws, predicted);

        // Assert
        result.Should().Be(0.0);
    }

    [Test]
    public void Given_History_And_Predicted_When_CalculateConsecutiveNumbersConfidence_Method_Is_Invoked_Should_Return_Correct_Ratio()
    {
        // Arrange
        // History pairs: [1,2] [2,3] => 2 in d1; [4,5] => 1 in d2; total = 3
        // Predicted pairs: [1,2] and [4,5] => intersect count = 2 => 2/3
        var draws = new[]
        {
            AlgorithmsTestHelperTests.Draw(1, 2, 3),
            AlgorithmsTestHelperTests.Draw(4, 5, 9)
        };
        var predicted = new List<int> { 0, 1, 2, 4, 5 };

        // Act
        var result = ConsecutiveNumbersAlgorithmHelpers.CalculateConsecutiveNumbersConfidence(draws, predicted);

        // Assert
        result.Should().BeApproximately(2.0 / 3.0, 1e-9);
    }
}