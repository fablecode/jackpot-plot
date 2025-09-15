using FluentAssertions;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Repositories;
using JackpotPlot.Domain.Services.PredictionStrategies;
using NSubstitute;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.ServicesTests.PredictionStrategiesTests.GapAnalysisPredictionStrategyTests;

[TestFixture]
public class PredictTests
{
    private ILotteryConfigurationRepository _configRepo = null!;
    private ILotteryHistoryRepository _historyRepo = null!;
    private GapAnalysisPredictionStrategy _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _configRepo = Substitute.For<ILotteryConfigurationRepository>();
        _historyRepo = Substitute.For<ILotteryHistoryRepository>();
        _sut = new GapAnalysisPredictionStrategy(_configRepo, _historyRepo);
    }

    // ---------- POSITIVE CASES ----------

    [Test]
    public async Task Given_Valid_Config_When_Predict_Is_Invoked_Should_Succeed()
    {
        // Arrange
        const int lotteryId = 801;
        var config = new LotteryConfigurationDomain
        {
            LotteryId = lotteryId,
            MainNumbersCount = 5,
            MainNumbersRange = 50,
            BonusNumbersCount = 0,
            BonusNumbersRange = 0
        };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);

        var draws = new List<HistoricalDraw>
        {
            new(1, lotteryId, DateTime.UtcNow.AddDays(-7), new List<int>{ 1, 5, 12, 20, 33 }, new List<int>(), DateTime.UtcNow),
            new(2, lotteryId, DateTime.UtcNow.AddDays(-5), new List<int>{ 2, 7, 15, 21, 40 }, new List<int>(), DateTime.UtcNow),
            new(3, lotteryId, DateTime.UtcNow.AddDays(-2), new List<int>{ 3, 8, 16, 22, 41 }, new List<int>(), DateTime.UtcNow),
        };
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(draws);

        // Act
        var result = await _sut.Predict(lotteryId);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task Given_Valid_Config_When_Predict_Is_Invoked_Should_Return_MainNumbers_In_Range()
    {
        // Arrange
        const int lotteryId = 802;
        var config = new LotteryConfigurationDomain
        {
            LotteryId = lotteryId,
            MainNumbersCount = 4,
            MainNumbersRange = 40,
            BonusNumbersCount = 0,
            BonusNumbersRange = 0
        };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);

        _historyRepo.GetHistoricalDraws(lotteryId).Returns(new List<HistoricalDraw>
        {
            new(1, lotteryId, DateTime.UtcNow.AddDays(-6), new List<int>{ 4, 10, 14, 25 }, new List<int>(), DateTime.UtcNow),
            new(2, lotteryId, DateTime.UtcNow.AddDays(-3), new List<int>{ 6, 12, 18, 30 }, new List<int>(), DateTime.UtcNow),
        });

        // Act
        var prediction = (await _sut.Predict(lotteryId)).Value!;

        // Assert
        prediction.PredictedNumbers.Should().OnlyContain(n => n >= 1 && n <= config.MainNumbersRange);
    }

    [Test]
    public async Task Given_Valid_Config_When_Predict_Is_Invoked_Should_Not_Exceed_MainNumbersCount()
    {
        // Arrange
        const int lotteryId = 803;
        var config = new LotteryConfigurationDomain
        {
            LotteryId = lotteryId,
            MainNumbersCount = 3,
            MainNumbersRange = 30,
            BonusNumbersCount = 0,
            BonusNumbersRange = 0
        };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);

        _historyRepo.GetHistoricalDraws(lotteryId).Returns(new List<HistoricalDraw>
        {
            new(1, lotteryId, DateTime.UtcNow.AddDays(-10), new List<int>{ 1, 2, 3 }, new List<int>(), DateTime.UtcNow),
            new(2, lotteryId, DateTime.UtcNow.AddDays(-8), new List<int>{ 2, 4, 6 }, new List<int>(), DateTime.UtcNow),
            new(3, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int>{ 3, 5, 7 }, new List<int>(), DateTime.UtcNow),
        });

        // Act
        var prediction = (await _sut.Predict(lotteryId)).Value!;

        // Assert
        prediction.PredictedNumbers.Count().Should().BeLessOrEqualTo(config.MainNumbersCount);
    }

    [Test]
    public async Task Given_Valid_Config_With_Bonuses_When_Predict_Is_Invoked_Should_Return_Configured_Bonus_Count()
    {
        // Arrange
        const int lotteryId = 804;
        var config = new LotteryConfigurationDomain
        {
            LotteryId = lotteryId,
            MainNumbersCount = 4,
            MainNumbersRange = 35,
            BonusNumbersCount = 2,
            BonusNumbersRange = 10
        };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);

        _historyRepo.GetHistoricalDraws(lotteryId).Returns(new List<HistoricalDraw>
        {
            new(1, lotteryId, DateTime.UtcNow.AddDays(-5), new List<int>{ 4, 9, 11, 22 }, new List<int>{ 1 }, DateTime.UtcNow),
            new(2, lotteryId, DateTime.UtcNow.AddDays(-2), new List<int>{ 5, 10, 12, 23 }, new List<int>{ 2 }, DateTime.UtcNow),
        });

        // Act
        var prediction = (await _sut.Predict(lotteryId)).Value!;

        // Assert
        prediction.BonusNumbers.Count().Should().Be(config.BonusNumbersCount);
    }

    [Test]
    public async Task Given_Valid_Config_With_Bonuses_When_Predict_Is_Invoked_Should_Return_BonusNumbers_In_Range()
    {
        // Arrange
        const int lotteryId = 805;
        var config = new LotteryConfigurationDomain
        {
            LotteryId = lotteryId,
            MainNumbersCount = 4,
            MainNumbersRange = 35,
            BonusNumbersCount = 2,
            BonusNumbersRange = 9
        };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);

        _historyRepo.GetHistoricalDraws(lotteryId).Returns(new List<HistoricalDraw>
        {
            new(1, lotteryId, DateTime.UtcNow.AddDays(-5), new List<int>{ 4, 9, 11, 22 }, new List<int>{ 1 }, DateTime.UtcNow),
            new(2, lotteryId, DateTime.UtcNow.AddDays(-2), new List<int>{ 5, 10, 12, 23 }, new List<int>{ 2 }, DateTime.UtcNow),
        });

        // Act
        var prediction = (await _sut.Predict(lotteryId)).Value!;

        // Assert
        prediction.BonusNumbers.Should().OnlyContain(n => n >= 1 && n <= config.BonusNumbersRange);
    }

    [Test]
    public async Task Given_Valid_Config_When_Predict_Is_Invoked_Should_Return_Confidence_In_Range()
    {
        // Arrange
        const int lotteryId = 806;
        var config = new LotteryConfigurationDomain
        {
            LotteryId = lotteryId,
            MainNumbersCount = 5,
            MainNumbersRange = 50,
            BonusNumbersCount = 0,
            BonusNumbersRange = 0
        };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);

        _historyRepo.GetHistoricalDraws(lotteryId).Returns(new List<HistoricalDraw>
        {
            new(1, lotteryId, DateTime.UtcNow.AddDays(-7), new List<int>{ 1, 12, 20, 33, 45 }, new List<int>(), DateTime.UtcNow),
            new(2, lotteryId, DateTime.UtcNow.AddDays(-4), new List<int>{ 2, 7, 15, 28, 40 }, new List<int>(), DateTime.UtcNow),
            new(3, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int>{ 3, 8, 16, 29, 41 }, new List<int>(), DateTime.UtcNow),
        });

        // Act
        var prediction = (await _sut.Predict(lotteryId)).Value!;

        // Assert
        prediction.ConfidenceScore.Should().BeInRange(0.0, 1.0);
    }

    // ---------- NEGATIVE / ERROR CASES ----------

    [Test]
    public async Task Given_No_Active_Configuration_When_Predict_Is_Invoked_Should_Fail()
    {
        // Arrange
        const int lotteryId = 811;
        _configRepo.GetActiveConfigurationAsync(lotteryId)
            .Returns(Task.FromResult<LotteryConfigurationDomain?>(null));

        // Act
        var result = await _sut.Predict(lotteryId);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    [Test]
    public async Task Given_No_Active_Configuration_When_Predict_Is_Invoked_Should_Return_Errors()
    {
        // Arrange
        const int lotteryId = 812;
        _configRepo.GetActiveConfigurationAsync(lotteryId)
            .Returns(Task.FromResult<LotteryConfigurationDomain?>(null));

        // Act
        var result = await _sut.Predict(lotteryId);

        // Assert
        result.Errors.Should().NotBeNullOrEmpty();
    }

    [Test]
    public async Task Given_No_Active_Configuration_When_Predict_Is_Invoked_Errors_Should_Contain_LotteryId()
    {
        // Arrange
        const int lotteryId = 813;
        _configRepo.GetActiveConfigurationAsync(lotteryId)
            .Returns(Task.FromResult<LotteryConfigurationDomain?>(null));

        // Act
        var result = await _sut.Predict(lotteryId);

        // Assert
        result.Errors.Should().Contain(e => e.Contains(lotteryId.ToString()));
    }

    [Test]
    public async Task Given_Empty_History_When_Predict_Is_Invoked_Should_Fail()
    {
        // Arrange
        const int lotteryId = 821;
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
        result.IsSuccess.Should().BeFalse();
    }

    [Test]
    public async Task Given_Empty_History_When_Predict_Is_Invoked_Should_Return_Errors()
    {
        // Arrange
        const int lotteryId = 822;
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
    public async Task Given_Empty_History_When_Predict_Is_Invoked_Errors_Should_Contain_LotteryId()
    {
        // Arrange
        const int lotteryId = 823;
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
}