using FluentAssertions;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.ClusteringAnalysisPredictionStrategyHelpersTests;

[TestFixture]
public class GenerateCoOccurrenceMatrixTests
{
    [Test]
    public void Given_An_Empty_HistoricalDraws_When_GenerateCoOccurrenceMatrix_Method_Is_Invoked_Then_All_Matrix_Values_Are_Zero()
    {
        // Arrange
        var historicalDraws = new List<HistoricalDraw>();
        const int numberRange = 5;

        // Act
        var matrix = ClusteringAnalysisPredictionStrategyHelpers.GenerateCoOccurrenceMatrix(historicalDraws, numberRange);

        // Assert: every cell in the (numberRange+1)x(numberRange+1) matrix should be 0.
        for (var i = 0; i < numberRange + 1; i++)
        {
            for (var j = 0; j < numberRange + 1; j++)
            {
                matrix[i, j].Should().Be(0);
            }
        }
    }

    [Test]
    public void Given_A_HistoricalDraw_With_Single_Number_When_GenerateCoOccurrenceMatrix_Method_Is_Invoked_Then_Matrix_Remains_Zero()
    {
        // Arrange
        var historicalDraws = new List<HistoricalDraw>
        {
            new(0, 0, default, [5], [], default)
        };

        const int numberRange = 10;

        // Act
        var matrix = ClusteringAnalysisPredictionStrategyHelpers.GenerateCoOccurrenceMatrix(historicalDraws, numberRange);

        // Assert: No pairs exist when there is only one winning number.
        for (var i = 0; i < numberRange + 1; i++)
        {
            for (var j = 0; j < numberRange + 1; j++)
            {
                matrix[i, j].Should().Be(0);
            }
        }
    }

    [Test]
    public void Given_A_Single_HistoricalDraw_With_Multiple_Numbers_When_GenerateCoOccurrenceMatrix_Method_Is_Invoked_Then_Matrix_Is_Populated_Symmetrically()
    {
        // Arrange
        // Using a single draw with winning numbers 1, 2, 3.
        var historicalDraws = new List<HistoricalDraw>
        {
            new(0, 0, default, [1, 2, 3], [], default)
        };

        const int numberRange = 5;

        // Act
        var matrix = ClusteringAnalysisPredictionStrategyHelpers.GenerateCoOccurrenceMatrix(historicalDraws, numberRange);

        // Assert: For draw {1,2,3}, the following pairs should be incremented:
        // (1,2), (1,3), and (2,3) each once, with symmetric entries updated.
        matrix[1, 2].Should().Be(1);
        matrix[2, 1].Should().Be(1);
        matrix[1, 3].Should().Be(1);
        matrix[3, 1].Should().Be(1);
        matrix[2, 3].Should().Be(1);
        matrix[3, 2].Should().Be(1);

        // Also assert that non-paired indices remain 0.
        matrix[1, 1].Should().Be(0);
        matrix[2, 2].Should().Be(0);
        matrix[3, 3].Should().Be(0);
    }

    [Test]
    public void Given_Multiple_HistoricalDraws_With_Overlapping_Numbers_When_GenerateCoOccurrenceMatrix_Method_Is_Invoked_Then_Matrix_Counts_Are_Summed_Correctly()
    {
        // Arrange
        // First draw with numbers: {1,2,3} and second with {2,3,4}.
        var historicalDraws = new List<HistoricalDraw>
        {
            new(0, 0, default, [1, 2, 3], [], default),
            new(0, 0, default, [2, 3, 4], [], default)
        };

        const int numberRange = 5;

        // Act
        var matrix = ClusteringAnalysisPredictionStrategyHelpers.GenerateCoOccurrenceMatrix(historicalDraws, numberRange);

        // Assert:
        // First draw adds: (1,2), (1,3), (2,3) each by 1.
        // Second draw adds: (2,3), (2,4), (3,4) each by 1.
        // Thus, pair (2,3) appears twice; others once.
        matrix[1, 2].Should().Be(1);
        matrix[2, 1].Should().Be(1);
        matrix[1, 3].Should().Be(1);
        matrix[3, 1].Should().Be(1);
        matrix[2, 3].Should().Be(2);
        matrix[3, 2].Should().Be(2);
        matrix[2, 4].Should().Be(1);
        matrix[4, 2].Should().Be(1);
        matrix[3, 4].Should().Be(1);
        matrix[4, 3].Should().Be(1);

        // Optionally, verify that cells not involved remain 0.
        matrix[1, 4].Should().Be(0);
        matrix[4, 1].Should().Be(0);
    }
}