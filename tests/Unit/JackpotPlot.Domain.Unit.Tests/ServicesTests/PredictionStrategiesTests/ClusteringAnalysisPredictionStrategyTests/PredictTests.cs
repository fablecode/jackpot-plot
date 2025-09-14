using FluentAssertions;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Repositories;
using JackpotPlot.Domain.Services.PredictionStrategies;
using NSubstitute;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.ServicesTests.PredictionStrategiesTests.ClusteringAnalysisPredictionStrategyTests;

[TestFixture]
public class PredictTests
{
    private ILotteryConfigurationRepository _configRepo = null!;
    private ILotteryHistoryRepository _historyRepo = null!;
    private ClusteringAnalysisPredictionStrategy _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _configRepo = Substitute.For<ILotteryConfigurationRepository>();
        _historyRepo = Substitute.For<ILotteryHistoryRepository>();
        _sut = new ClusteringAnalysisPredictionStrategy(_configRepo, _historyRepo);
    }

    [Test]
    public async Task Given_No_Active_Configuration_When_Predict_Method_Is_Invoked_ISuccess_Should_Be_False()
    {
        // Arrange
        const int lotteryId = 42;
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(Task.FromResult<LotteryConfigurationDomain?>(null));

        // Act
        var result = await _sut.Predict(lotteryId);

        // Assert
        result.IsSuccess.Should().BeFalse("no configuration exists for the given lottery");
    }

    [Test]
    public async Task Given_No_Active_Configuration_When_Predict_Method_Is_Invoked_Should_Return_Errors()
    {
        // Arrange
        const int lotteryId = 42;
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(Task.FromResult<LotteryConfigurationDomain?>(null));

        // Act
        var result = await _sut.Predict(lotteryId);

        // Assert
        result.Errors.Should().NotBeNullOrEmpty();
    }

    [Test]
    public async Task Given_No_Active_Configuration_When_Predict_Method_Is_Invoked_Should_Return_Errors_With_A_Message_Containing_LotteryId()
    {
        // Arrange
        const int lotteryId = 42;
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(Task.FromResult<LotteryConfigurationDomain?>(null));

        // Act
        var result = await _sut.Predict(lotteryId);

        // Assert
        result.Errors.Should().Contain(e => e.Contains(lotteryId.ToString()));
    }

    [Test]
    public async Task Given_Empty_History_When_Predict_Method_Is_Invoked_IsSuccess_Should_Be_False()
    {
        // Arrange
        const int lotteryId = 7;

        var config = new LotteryConfigurationDomain
        {
            LotteryId = lotteryId,
            MainNumbersCount = 5,
            MainNumbersRange = 50,
            BonusNumbersCount = 0,
            BonusNumbersRange = 0
        };

        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(Task.FromResult<ICollection<Models.HistoricalDraw>>(new List<JackpotPlot.Domain.Models.HistoricalDraw>()));

        // Act
        var result = await _sut.Predict(lotteryId);

        // Assert
        result.IsSuccess.Should().BeFalse("no historical draws are available");
    }

    [Test]
    public async Task Given_Empty_History_When_Predict_Method_Is_Invoked_Should_Return_Errors()
    {
        // Arrange
        const int lotteryId = 7;

        var config = new LotteryConfigurationDomain
        {
            LotteryId = lotteryId,
            MainNumbersCount = 5,
            MainNumbersRange = 50,
            BonusNumbersCount = 0,
            BonusNumbersRange = 0
        };

        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(Task.FromResult<ICollection<Models.HistoricalDraw>>(new List<JackpotPlot.Domain.Models.HistoricalDraw>()));

        // Act
        var result = await _sut.Predict(lotteryId);

        // Assert
        result.Errors.Should().NotBeNullOrEmpty();
    }

    [Test]
    public async Task Given_Empty_History_When_Predict_Method_Is_Invoked_Should_Return_Errors_With_Message_Containing_LotteryId()
    {
        // Arrange
        const int lotteryId = 7;

        var config = new LotteryConfigurationDomain
        {
            LotteryId = lotteryId,
            MainNumbersCount = 5,
            MainNumbersRange = 50,
            BonusNumbersCount = 0,
            BonusNumbersRange = 0
        };

        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(Task.FromResult<ICollection<Models.HistoricalDraw>>(new List<JackpotPlot.Domain.Models.HistoricalDraw>()));

        // Act
        var result = await _sut.Predict(lotteryId);

        // Assert
        result.Errors.Should().Contain(e => e.Contains(lotteryId.ToString()));
    }

    [Test]
    public async Task Given_Valid_Config_And_No_Bonus_When_Predict_Method_Is_Invoked_IsSuccess_Should_Be_True()
    {
        // Arrange
        const int lotteryId = 10;

        var config = new LotteryConfigurationDomain
        {
            LotteryId = lotteryId,
            MainNumbersCount = 5,
            MainNumbersRange = 35,
            BonusNumbersCount = 0,
            BonusNumbersRange = 0
        };

        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);

        var draws = new List<Models.HistoricalDraw>
        {
            // Minimal realistic history; exact values don't matter due to randomized clustering.
            new(1, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int> { 1, 2, 3, 4, 5 }, new List<int>(), DateTime.UtcNow)
        };

        _historyRepo.GetHistoricalDraws(lotteryId).Returns(draws);

        // Act
        var result = await _sut.Predict(lotteryId);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task Given_Valid_Config_And_No_Bonus_When_Predict_Method_Is_Invoked_PredictionResult_Should_Not_Be_Null()
    {
        // Arrange
        const int lotteryId = 10;

        var config = new LotteryConfigurationDomain
        {
            LotteryId = lotteryId,
            MainNumbersCount = 5,
            MainNumbersRange = 35,
            BonusNumbersCount = 0,
            BonusNumbersRange = 0
        };

        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);

        var draws = new List<Models.HistoricalDraw>
        {
            // Minimal realistic history; exact values don't matter due to randomized clustering.
            new(1, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int> { 1, 2, 3, 4, 5 }, new List<int>(), DateTime.UtcNow)
        };

        _historyRepo.GetHistoricalDraws(lotteryId).Returns(draws);

        // Act
        var result = await _sut.Predict(lotteryId);

        // Assert
        result.Value.Should().NotBeNull();
    }

    [Test]
    public async Task Given_Valid_Config_And_No_Bonus_When_Predict_Method_Is_Invoked_PredictedNumbers_Should_Be_Not_Null_Or_Empty()
    {
        // Arrange
        const int lotteryId = 10;

        var config = new LotteryConfigurationDomain
        {
            LotteryId = lotteryId,
            MainNumbersCount = 5,
            MainNumbersRange = 35,
            BonusNumbersCount = 0,
            BonusNumbersRange = 0
        };

        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);

        var draws = new List<Models.HistoricalDraw>
        {
            // Minimal realistic history; exact values don't matter due to randomized clustering.
            new(1, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int> { 1, 2, 3, 4, 5 }, new List<int>(), DateTime.UtcNow)
        };

        _historyRepo.GetHistoricalDraws(lotteryId).Returns(draws);

        // Act
        var result = await _sut.Predict(lotteryId);

        // Assert
        result.Value.PredictedNumbers.Should().NotBeNull();
    }

    [Test]
    public async Task Given_Valid_Config_And_No_Bonus_When_Predict_Method_Is_Invoked_PredictedNumbers_Length_Should_Be_Less_Or_Equal_To_MainNumbersCount()
    {
        // Arrange
        const int lotteryId = 10;

        var config = new LotteryConfigurationDomain
        {
            LotteryId = lotteryId,
            MainNumbersCount = 5,
            MainNumbersRange = 35,
            BonusNumbersCount = 0,
            BonusNumbersRange = 0
        };

        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);

        var draws = new List<Models.HistoricalDraw>
        {
            // Minimal realistic history; exact values don't matter due to randomized clustering.
            new(1, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int> { 1, 2, 3, 4, 5 }, new List<int>(), DateTime.UtcNow)
        };

        _historyRepo.GetHistoricalDraws(lotteryId).Returns(draws);

        // Act
        var result = await _sut.Predict(lotteryId);

        // Assert
        result.Value.PredictedNumbers.Length.Should().BeLessOrEqualTo(config.MainNumbersCount);

    }

    [Test]
    public async Task Given_Valid_Config_And_No_Bonus_When_Predict_Is_Invoked_Should_Return_MainNumbers_Within_MainNumbersRange()
    {
        // Arrange
        const int lotteryId = 10;

        var config = new LotteryConfigurationDomain
        {
            LotteryId = lotteryId,
            MainNumbersCount = 5,
            MainNumbersRange = 35,
            BonusNumbersCount = 0,
            BonusNumbersRange = 0
        };

        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);

        var draws = new List<Models.HistoricalDraw>
        {
            // Minimal realistic history; exact values don't matter due to randomized clustering.
            new(1, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int> { 1, 2, 3, 4, 5 }, new List<int>(), DateTime.UtcNow)
        };

        _historyRepo.GetHistoricalDraws(lotteryId).Returns(draws);

        // Act
        var result = await _sut.Predict(lotteryId);

        // Assert
        result.Value.PredictedNumbers.Should().OnlyContain(n => n >= 1 && n <= config.MainNumbersRange);
    }

    [Test]
    public async Task Given_Valid_Config_And_No_Bonus_When_Predict_Is_Invoked_Should_Return_ConfidenceScore_Of_One()
    {
        // Arrange
        const int lotteryId = 10;

        var config = new LotteryConfigurationDomain
        {
            LotteryId = lotteryId,
            MainNumbersCount = 5,
            MainNumbersRange = 35,
            BonusNumbersCount = 0,
            BonusNumbersRange = 0
        };

        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);

        var draws = new List<Models.HistoricalDraw>
        {
            // Minimal realistic history; exact values don't matter due to randomized clustering.
            new(1, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int> { 1, 2, 3, 4, 5 }, new List<int>(), DateTime.UtcNow)
        };

        _historyRepo.GetHistoricalDraws(lotteryId).Returns(draws);

        // Act
        var result = await _sut.Predict(lotteryId);

        // Assert
        // Confidence should be 1.0 because selections are always from clusters.
        result.Value.ConfidenceScore.Should().Be(1.0);
    }

    [Test]
    public async Task Given_Valid_Config_And_No_Bonus_When_Predict_Is_Invoked_Should_Return_Empty_BonusNumbers()
    {
        // Arrange
        const int lotteryId = 10;

        var config = new LotteryConfigurationDomain
        {
            LotteryId = lotteryId,
            MainNumbersCount = 5,
            MainNumbersRange = 35,
            BonusNumbersCount = 0,
            BonusNumbersRange = 0
        };

        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);

        var draws = new List<Models.HistoricalDraw>
        {
            // Minimal realistic history; exact values don't matter due to randomized clustering.
            new(1, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int> { 1, 2, 3, 4, 5 }, new List<int>(), DateTime.UtcNow)
        };

        _historyRepo.GetHistoricalDraws(lotteryId).Returns(draws);

        // Act
        var result = await _sut.Predict(lotteryId);

        // Assert
        result.Value.BonusNumbers.Should().BeEmpty();
    }

    [Test]
    public async Task Given_Valid_Config_With_Bonus_When_Predict_Is_Invoked_BonusNumbers_Must_Match_Requested_Count()
    {
        // Arrange
        const int lotteryId = 11;

        var config = new LotteryConfigurationDomain
        {
            LotteryId = lotteryId,
            MainNumbersCount = 6,
            MainNumbersRange = 49,
            BonusNumbersCount = 2,
            BonusNumbersRange = 10
        };

        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);

        var draws = new List<Models.HistoricalDraw>
        {
            new(1, lotteryId, DateTime.UtcNow.AddDays(-3), new List<int> { 7, 14, 21, 28, 35, 42 }, new List<int>(), DateTime.UtcNow),
            new(2, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int> { 3, 9, 12, 18, 27, 33 }, new List<int>(), DateTime.UtcNow)
        };

        _historyRepo.GetHistoricalDraws(lotteryId).Returns(draws);

        // Act
        var result = await _sut.Predict(lotteryId);

        // Assert
        result.Value.BonusNumbers.Length.Should().Be(config.BonusNumbersCount);
    }

    [Test]
    public async Task Given_Valid_Config_With_Bonus_When_Predict_Is_Invoked_BonusNumbers_OnlyContain_Numbers_Within_BonusNumbers_Range()
    {
        // Arrange
        const int lotteryId = 11;

        var config = new LotteryConfigurationDomain
        {
            LotteryId = lotteryId,
            MainNumbersCount = 6,
            MainNumbersRange = 49,
            BonusNumbersCount = 2,
            BonusNumbersRange = 10
        };

        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);

        var draws = new List<Models.HistoricalDraw>
        {
            new(1, lotteryId, DateTime.UtcNow.AddDays(-3), new List<int> { 7, 14, 21, 28, 35, 42 }, new List<int>(), DateTime.UtcNow),
            new(2, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int> { 3, 9, 12, 18, 27, 33 }, new List<int>(), DateTime.UtcNow)
        };

        _historyRepo.GetHistoricalDraws(lotteryId).Returns(draws);

        // Act
        var result = await _sut.Predict(lotteryId);

        // Assert
        result.Value.BonusNumbers.Should().OnlyContain(n => n >= 1 && n <= config.BonusNumbersRange);
    }
}