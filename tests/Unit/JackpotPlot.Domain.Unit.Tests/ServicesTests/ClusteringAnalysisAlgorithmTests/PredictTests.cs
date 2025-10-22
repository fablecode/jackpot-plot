using FluentAssertions;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Predictions.Algorithms;
using JackpotPlot.Primitives.Algorithms;
using NSubstitute;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.ServicesTests.ClusteringAnalysisAlgorithmTests;

[TestFixture]
public class PredictTests
{
    [Test]
    public void Given_Valid_Config_And_Empty_History_When_Predict_Is_Called_Should_Return_Main_Count_Equals_Config()
    {
        // Arrange
        var config = CreateConfig(mainRange: 50, mainCount: 5);
        var history = EmptyHistoryWithNSubstitute();
        var sut = new ClusteringAnalysisAlgorithm();
        var random = new Random(123);

        // Act
        var result = sut.Predict(config, history, random);

        // Assert
        result.PredictedNumbers.Length.Should().Be(config.MainNumbersCount);
    }

    [Test]
    public void Given_Bonus_Count_Zero_When_Predict_Is_Called_Should_Return_Empty_Bonus()
    {
        // Arrange
        var config = CreateConfig(bonusCount: 0);
        var history = EmptyHistoryWithNSubstitute();
        var sut = new ClusteringAnalysisAlgorithm();
        var random = new Random(123);

        // Act
        var result = sut.Predict(config, history, random);

        // Assert
        result.BonusNumbers.IsEmpty.Should().BeTrue();
    }

    [Test]
    public void Given_Bonus_Count_Positive_When_Predict_Is_Called_Should_Return_Bonus_Count_Equals_Config()
    {
        // Arrange
        var config = CreateConfig(bonusCount: 2, bonusRange: 12);
        var history = EmptyHistoryWithNSubstitute();
        var sut = new ClusteringAnalysisAlgorithm();
        var random = new Random(123);

        // Act
        var result = sut.Predict(config, history, random);

        // Assert
        result.BonusNumbers.Length.Should().Be(config.BonusNumbersCount);
    }

    [Test]
    public void Given_Valid_Config_When_Predict_Is_Called_Should_Set_AlgorithmKey_To_ClusteringAnalysis()
    {
        // Arrange
        var config = CreateConfig();
        var history = EmptyHistoryWithNSubstitute();
        var sut = new ClusteringAnalysisAlgorithm();
        var random = new Random(123);

        // Act
        var result = sut.Predict(config, history, random);

        // Assert
        result.AlgorithmKey.Should().Be(PredictionAlgorithmKeys.ClusteringAnalysis);
    }

    [Test]
    public void Given_Valid_Config_When_Predict_Is_Called_Should_Preserve_LotteryId()
    {
        // Arrange
        const int lotteryId = 6;
        var config = CreateConfig(lotteryId: lotteryId);
        var history = EmptyHistoryWithNSubstitute();
        var sut = new ClusteringAnalysisAlgorithm();
        var random = new Random(123);

        // Act
        var result = sut.Predict(config, history, random);

        // Assert
        result.LotteryId.Should().Be(lotteryId);
    }

    [Test]
    public void Given_PredictedNumbers_Range_When_Predict_Is_Called_Should_Return_PredictedNumbers_In_Range()
    {
        // Arrange
        var config = CreateConfig(mainRange: 40, mainCount: 6);
        var history = EmptyHistoryWithNSubstitute();
        var sut = new ClusteringAnalysisAlgorithm();
        var random = new Random(456);

        // Act
        var result = sut.Predict(config, history, random);

        // Assert
        result.PredictedNumbers.All(n => n >= 1 && n <= config.MainNumbersRange).Should().BeTrue();
    }

    [Test]
    public void Given_BonusNumbers_Range_When_Predict_Is_Called_Should_Return_BonusNumbers_In_Range()
    {
        // Arrange
        var config = CreateConfig(bonusCount: 3, bonusRange: 7);
        var history = EmptyHistoryWithNSubstitute();
        var sut = new ClusteringAnalysisAlgorithm();
        var random = new Random(789);

        // Act
        var result = sut.Predict(config, history, random);

        // Assert
        result.BonusNumbers.All(n => n >= 1 && n <= config.BonusNumbersRange).Should().BeTrue();
    }

    [Test]
    public void Given_Selected_PredictedNumbers_Comes_From_Clusters_When_Predict_Is_Called_Should_Return_Confidence_Equal_1()
    {
        // Arrange
        // NOTE: PerformClustering + SelectNumbersFromClusters always pick main numbers from the produced clusters,
        // so confidence = correct / total = 1.
        var config = CreateConfig(mainRange: 30, mainCount: 5);
        var history = EmptyHistoryWithNSubstitute();
        var sut = new ClusteringAnalysisAlgorithm();
        var random = new Random(123);

        // Act
        var result = sut.Predict(config, history, random);

        // Assert
        result.ConfidenceScore.Should().Be(1.0);
    }

    #region Test Helpers

    private static LotteryConfigurationDomain CreateConfig(
    int lotteryId = 8,
    int mainRange = 50,
    int mainCount = 5,
    int bonusRange = 10,
    int bonusCount = 0)
    {
        // Assumes these properties are settable on your domain type.
        // If your type uses a ctor, adapt accordingly in your codebase.
        return new LotteryConfigurationDomain
        {
            LotteryId = lotteryId,
            MainNumbersRange = mainRange,
            MainNumbersCount = mainCount,
            BonusNumbersRange = bonusRange,
            BonusNumbersCount = bonusCount
        };
    }

    private static IReadOnlyList<HistoricalDraw> EmptyHistoryWithNSubstitute()
    {
        // Use NSubstitute to satisfy the requirement and to ensure
        // IEnumerable iteration won’t yield elements.
        var history = Substitute.For<IReadOnlyList<HistoricalDraw>>();
        history.Count.Returns(0);
        using var returnThis = Enumerable.Empty<HistoricalDraw>().GetEnumerator();
        using var enumerator = history.GetEnumerator();
        enumerator.Returns(returnThis);
        return history;
    } 

    #endregion
}