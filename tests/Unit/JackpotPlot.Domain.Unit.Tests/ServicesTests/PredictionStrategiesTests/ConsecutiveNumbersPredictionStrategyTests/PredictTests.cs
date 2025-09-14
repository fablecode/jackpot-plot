using FluentAssertions;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Repositories;
using JackpotPlot.Domain.Services.PredictionStrategies;
using NSubstitute;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.ServicesTests.PredictionStrategiesTests.ConsecutiveNumbersPredictionStrategyTests;

[TestFixture]
public class PredictTests
{
    private ILotteryConfigurationRepository _configRepo = null!;
    private ILotteryHistoryRepository _historyRepo = null!;
    private ConsecutiveNumbersPredictionStrategy _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _configRepo = Substitute.For<ILotteryConfigurationRepository>();
        _historyRepo = Substitute.For<ILotteryHistoryRepository>();
        _sut = new ConsecutiveNumbersPredictionStrategy(_configRepo, _historyRepo);
    }

    [Test]
    public async Task Given_No_Active_Configuration_When_Predict_Method_Is_Invoked_IsSuccess_Should_Be_False()
    {
        // Arrange
        const int lotteryId = 101;
        _configRepo.GetActiveConfigurationAsync(lotteryId)
            .Returns(Task.FromResult<LotteryConfigurationDomain?>(null));

        // Act
        var result = await _sut.Predict(lotteryId);

        // Assert
        result.IsSuccess.Should().BeFalse("no configuration exists for the given lottery");
    }

    [Test]
    public async Task Given_No_Active_Configuration_When_Predict_Method_Is_Invoked_Should_Return_Errors()
    {
        // Arrange
        const int lotteryId = 101;
        _configRepo.GetActiveConfigurationAsync(lotteryId)
            .Returns(Task.FromResult<LotteryConfigurationDomain?>(null));

        // Act
        var result = await _sut.Predict(lotteryId);

        // Assert
        result.Errors.Should().NotBeNullOrEmpty();
    }

    [Test]
    public async Task Given_No_Active_Configuration_When_Predict_Method_Is_Invoked_Error_Message_Should_Contain_Lottery_Id()
    {
        // Arrange
        const int lotteryId = 101;
        _configRepo.GetActiveConfigurationAsync(lotteryId)
            .Returns(Task.FromResult<LotteryConfigurationDomain?>(null));

        // Act
        var result = await _sut.Predict(lotteryId);

        // Assert
        result.Errors.Should().Contain(e => e.Contains(lotteryId.ToString()));
    }

    [Test]
    public async Task Given_An_Empty_History_When_Predict_Method_Is_Invoked_IsSuccess_Should_Be_False()
    {
        // Arrange
        const int lotteryId = 202;

        var config = new LotteryConfigurationDomain
        {
            LotteryId = lotteryId,
            MainNumbersCount = 5,
            MainNumbersRange = 50,
            BonusNumbersCount = 0,
            BonusNumbersRange = 0
        };

        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        _historyRepo.GetHistoricalDraws(lotteryId)
            .Returns(Task.FromResult<ICollection<HistoricalDraw>>(new List<HistoricalDraw>()));

        // Act
        var result = await _sut.Predict(lotteryId);

        // Assert
        result.IsSuccess.Should().BeFalse("no historical draws are available");
    }

    [Test]
    public async Task Given_An_Empty_History_When_Predict_Method_Is_Invoked_Should_Return_Errors()
    {
        // Arrange
        const int lotteryId = 202;

        var config = new LotteryConfigurationDomain
        {
            LotteryId = lotteryId,
            MainNumbersCount = 5,
            MainNumbersRange = 50,
            BonusNumbersCount = 0,
            BonusNumbersRange = 0
        };

        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        _historyRepo.GetHistoricalDraws(lotteryId)
            .Returns(Task.FromResult<ICollection<HistoricalDraw>>(new List<HistoricalDraw>()));

        // Act
        var result = await _sut.Predict(lotteryId);

        // Assert
        result.Errors.Should().NotBeNullOrEmpty();
    }

    [Test]
    public async Task Given_An_Empty_History_When_Predict_Method_Is_Invoked_Error_Message_Should_Contain_Lottery_Id()
    {
        // Arrange
        const int lotteryId = 202;

        var config = new LotteryConfigurationDomain
        {
            LotteryId = lotteryId,
            MainNumbersCount = 5,
            MainNumbersRange = 50,
            BonusNumbersCount = 0,
            BonusNumbersRange = 0
        };

        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        _historyRepo.GetHistoricalDraws(lotteryId)
            .Returns(Task.FromResult<ICollection<HistoricalDraw>>(new List<HistoricalDraw>()));

        // Act
        var result = await _sut.Predict(lotteryId);

        // Assert
        result.Errors.Should().Contain(e => e.Contains(lotteryId.ToString()));
    }

    [Test]
    public async Task Given_Valid_Config_And_No_Bonus_When_Predict_Is_Invoked_IsSuccess_Should_Be_True()
    {
        // Arrange
        const int lotteryId = 303;

        var config = new LotteryConfigurationDomain
        {
            LotteryId = lotteryId,
            MainNumbersCount = 5,
            MainNumbersRange = 35,
            BonusNumbersCount = 0,
            BonusNumbersRange = 0
        };

        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);

        var draws = new List<HistoricalDraw>
            {
                new(
                    DrawId: 1,
                    LotteryId: lotteryId,
                    DrawDate: DateTime.UtcNow.AddDays(-2),
                    WinningNumbers: new List<int> { 1, 2, 3, 10, 20 },
                    BonusNumbers: new List<int>(),
                    CreatedAt: DateTime.UtcNow.AddDays(-2))
            };

        _historyRepo.GetHistoricalDraws(lotteryId).Returns(draws);

        // Act
        var result = await _sut.Predict(lotteryId);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task Given_Valid_Config_And_No_Bonus_When_Predict_Is_Invoked_PredictionResult_Should_Not_Be_Null()
    {
        // Arrange
        const int lotteryId = 303;

        var config = new LotteryConfigurationDomain
        {
            LotteryId = lotteryId,
            MainNumbersCount = 5,
            MainNumbersRange = 35,
            BonusNumbersCount = 0,
            BonusNumbersRange = 0
        };

        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);

        var draws = new List<HistoricalDraw>
            {
                new(
                    DrawId: 1,
                    LotteryId: lotteryId,
                    DrawDate: DateTime.UtcNow.AddDays(-2),
                    WinningNumbers: new List<int> { 1, 2, 3, 10, 20 },
                    BonusNumbers: new List<int>(),
                    CreatedAt: DateTime.UtcNow.AddDays(-2))
            };

        _historyRepo.GetHistoricalDraws(lotteryId).Returns(draws);

        // Act
        var result = await _sut.Predict(lotteryId);

        // Assert
        result.Value.Should().NotBeNull();
    }

    [Test]
    public async Task Given_Valid_Config_And_No_Bonus_When_Predict_Is_Invoked_PredictedNumbers_Should_Not_Be_Null_Or_Empty()
    {
        // Arrange
        const int lotteryId = 303;

        var config = new LotteryConfigurationDomain
        {
            LotteryId = lotteryId,
            MainNumbersCount = 5,
            MainNumbersRange = 35,
            BonusNumbersCount = 0,
            BonusNumbersRange = 0
        };

        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);

        var draws = new List<HistoricalDraw>
            {
                new(
                    DrawId: 1,
                    LotteryId: lotteryId,
                    DrawDate: DateTime.UtcNow.AddDays(-2),
                    WinningNumbers: new List<int> { 1, 2, 3, 10, 20 },
                    BonusNumbers: new List<int>(),
                    CreatedAt: DateTime.UtcNow.AddDays(-2))
            };

        _historyRepo.GetHistoricalDraws(lotteryId).Returns(draws);

        // Act
        var result = await _sut.Predict(lotteryId);

        // Assert
        result.Value.PredictedNumbers.Should().NotBeNullOrEmpty();
    }

    [Test]
    public async Task Given_Valid_Config_And_No_Bonus_When_Predict_Is_Invoked_PredictedNumbers_Should_Not_Only_Contain_Numbers_Between_MainNumbersRange()
    {
        // Arrange
        const int lotteryId = 303;

        var config = new LotteryConfigurationDomain
        {
            LotteryId = lotteryId,
            MainNumbersCount = 5,
            MainNumbersRange = 35,
            BonusNumbersCount = 0,
            BonusNumbersRange = 0
        };

        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);

        var draws = new List<HistoricalDraw>
            {
                new(
                    DrawId: 1,
                    LotteryId: lotteryId,
                    DrawDate: DateTime.UtcNow.AddDays(-2),
                    WinningNumbers: new List<int> { 1, 2, 3, 10, 20 },
                    BonusNumbers: new List<int>(),
                    CreatedAt: DateTime.UtcNow.AddDays(-2))
            };

        _historyRepo.GetHistoricalDraws(lotteryId).Returns(draws);

        // Act
        var result = await _sut.Predict(lotteryId);

        // Assert
        result.Value.PredictedNumbers.Should().OnlyContain(n => n >= 1 && n <= config.MainNumbersRange);
    }

    [Test]
    public async Task Given_Valid_Config_And_No_Bonus_When_Predict_Is_Invoked_PredictedNumbers_Confidence_Score_Should_BeLessOrEqualTo_1()
    {
        // Arrange
        const int lotteryId = 303;

        var config = new LotteryConfigurationDomain
        {
            LotteryId = lotteryId,
            MainNumbersCount = 5,
            MainNumbersRange = 35,
            BonusNumbersCount = 0,
            BonusNumbersRange = 0
        };

        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);

        var draws = new List<HistoricalDraw>
            {
                new(
                    DrawId: 1,
                    LotteryId: lotteryId,
                    DrawDate: DateTime.UtcNow.AddDays(-2),
                    WinningNumbers: new List<int> { 1, 2, 3, 10, 20 },
                    BonusNumbers: new List<int>(),
                    CreatedAt: DateTime.UtcNow.AddDays(-2))
            };

        _historyRepo.GetHistoricalDraws(lotteryId).Returns(draws);

        // Act
        var result = await _sut.Predict(lotteryId);

        // Assert
        // Confidence is computed inside the strategy; just assert it's a valid probability.
        result.Value.ConfidenceScore.Should().BeGreaterOrEqualTo(0.0).And.BeLessOrEqualTo(1.0);
    }
    [Test]
    public async Task Given_Valid_Config_And_No_Bonus_When_Predict_Is_Invoked_Should_Return_Empty_BonusNumbers()
    {
        // Arrange
        const int lotteryId = 303;

        var config = new LotteryConfigurationDomain
        {
            LotteryId = lotteryId,
            MainNumbersCount = 5,
            MainNumbersRange = 35,
            BonusNumbersCount = 0,
            BonusNumbersRange = 0
        };

        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);

        var draws = new List<HistoricalDraw>
            {
                new(
                    DrawId: 1,
                    LotteryId: lotteryId,
                    DrawDate: DateTime.UtcNow.AddDays(-2),
                    WinningNumbers: new List<int> { 1, 2, 3, 10, 20 },
                    BonusNumbers: new List<int>(),
                    CreatedAt: DateTime.UtcNow.AddDays(-2))
            };

        _historyRepo.GetHistoricalDraws(lotteryId).Returns(draws);

        // Act
        var result = await _sut.Predict(lotteryId);

        // Assert
        result.Value.BonusNumbers.Should().BeEmpty();
    }

    [Test]
    public async Task Given_Valid_Config_With_Bonus_When_Predict_Is_Invoked_Should_Return_Success_With_BonusNumbers_In_Range()
    {
        // Arrange
        const int lotteryId = 404;

        var config = new LotteryConfigurationDomain
        {
            LotteryId = lotteryId,
            MainNumbersCount = 6,
            MainNumbersRange = 49,
            BonusNumbersCount = 2,
            BonusNumbersRange = 12
        };

        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);

        var draws = new List<HistoricalDraw>
            {
                new(
                    DrawId: 10,
                    LotteryId: lotteryId,
                    DrawDate: DateTime.UtcNow.AddDays(-7),
                    WinningNumbers: new List<int> { 5, 6, 7, 20, 33, 40 },
                    BonusNumbers: new List<int>(),
                    CreatedAt: DateTime.UtcNow.AddDays(-7)),

                new(
                    DrawId: 11,
                    LotteryId: lotteryId,
                    DrawDate: DateTime.UtcNow.AddDays(-3),
                    WinningNumbers: new List<int> { 10, 11, 12, 13, 21, 30 },
                    BonusNumbers: new List<int>(),
                    CreatedAt: DateTime.UtcNow.AddDays(-3))
            };

        _historyRepo.GetHistoricalDraws(lotteryId).Returns(draws);

        // Act
        var result = await _sut.Predict(lotteryId);

        // Assert
        result.Value.BonusNumbers.Count().Should().Be(config.BonusNumbersCount);
    }

    [Test]
    public async Task Given_Valid_Config_With_Bonus_When_Predict_Is_Invoked_BonusNumbers_OnlyContain_Numbers_Within_BonusNumbers_Range()
    {
        // Arrange
        const int lotteryId = 404;

        var config = new LotteryConfigurationDomain
        {
            LotteryId = lotteryId,
            MainNumbersCount = 6,
            MainNumbersRange = 49,
            BonusNumbersCount = 2,
            BonusNumbersRange = 12
        };

        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);

        var draws = new List<HistoricalDraw>
            {
                new(
                    DrawId: 10,
                    LotteryId: lotteryId,
                    DrawDate: DateTime.UtcNow.AddDays(-7),
                    WinningNumbers: new List<int> { 5, 6, 7, 20, 33, 40 },
                    BonusNumbers: new List<int>(),
                    CreatedAt: DateTime.UtcNow.AddDays(-7)),

                new(
                    DrawId: 11,
                    LotteryId: lotteryId,
                    DrawDate: DateTime.UtcNow.AddDays(-3),
                    WinningNumbers: new List<int> { 10, 11, 12, 13, 21, 30 },
                    BonusNumbers: new List<int>(),
                    CreatedAt: DateTime.UtcNow.AddDays(-3))
            };

        _historyRepo.GetHistoricalDraws(lotteryId).Returns(draws);

        // Act
        var result = await _sut.Predict(lotteryId);

        // Assert
        result.Value.BonusNumbers.Should().OnlyContain(n => n >= 1 && n <= config.BonusNumbersRange);
    }
}