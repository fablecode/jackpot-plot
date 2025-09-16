using FluentAssertions;
using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Repositories;
using JackpotPlot.Domain.Services.PredictionStrategies;
using NSubstitute;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.ServicesTests.PredictionStrategiesTests.NumberSumPredictionStrategyTests;

[TestFixture]
public class PredictTests
{
    private ILotteryConfigurationRepository _configRepo = null!;
    private ILotteryHistoryRepository _historyRepo = null!;
    private NumberSumPredictionStrategy _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _configRepo = Substitute.For<ILotteryConfigurationRepository>();
        _historyRepo = Substitute.For<ILotteryHistoryRepository>();
        _sut = new NumberSumPredictionStrategy(_configRepo, _historyRepo);
    }

    // ---------- POSITIVE: base success ----------

    [Test]
    public async Task Given_Valid_Config_And_History_When_Predict_Is_Invoked_Should_Succeed()
    {
        // Arrange
        const int lotteryId = 9101;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 5, MainNumbersRange = 30, BonusNumbersCount = 0, BonusNumbersRange = 0 };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(new List<HistoricalDraw>
            {
                new(1, lotteryId, DateTime.UtcNow.AddDays(-3), new List<int>{ 3, 6, 9, 12, 15 }, new List<int>(), DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int>{ 4, 7, 10, 13, 16 }, new List<int>(), DateTime.UtcNow),
            });

        // Act
        var result = await _sut.Predict(lotteryId);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task Given_Valid_Config_And_History_When_Predict_Is_Invoked_Should_Return_Value()
    {
        // Arrange
        const int lotteryId = 9101;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 5, MainNumbersRange = 30, BonusNumbersCount = 0, BonusNumbersRange = 0 };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(new List<HistoricalDraw>
            {
                new(1, lotteryId, DateTime.UtcNow.AddDays(-3), new List<int>{ 3, 6, 9, 12, 15 }, new List<int>(), DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int>{ 4, 7, 10, 13, 16 }, new List<int>(), DateTime.UtcNow),
            });

        // Act
        var result = await _sut.Predict(lotteryId);

        // Assert
        result.Value.Should().NotBeNull();
    }

    // ---------- POSITIVE: main numbers ----------

    [Test]
    public async Task Given_Valid_Config_When_Predict_Is_Invoked_Should_Return_Exactly_Configured_Main_Count()
    {
        // Arrange
        const int lotteryId = 9102;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 4, MainNumbersRange = 25, BonusNumbersCount = 0, BonusNumbersRange = 0 };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(new List<HistoricalDraw>
            {
                new(1, lotteryId, DateTime.UtcNow.AddDays(-2), new List<int>{ 2, 6, 12, 18, 20 }, new List<int>(), DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int>{ 3, 7, 11, 15, 21 }, new List<int>(), DateTime.UtcNow),
            });

        // Act
        var prediction = (await _sut.Predict(lotteryId)).Value!;

        // Assert
        prediction.PredictedNumbers.Count().Should().Be(config.MainNumbersCount);
    }

    [Test]
    public async Task Given_Valid_Config_When_Predict_Is_Invoked_Should_Return_MainNumbers_In_Range()
    {
        // Arrange
        const int lotteryId = 9102;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 4, MainNumbersRange = 25, BonusNumbersCount = 0, BonusNumbersRange = 0 };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(new List<HistoricalDraw>
            {
                new(1, lotteryId, DateTime.UtcNow.AddDays(-2), new List<int>{ 2, 6, 12, 18, 20 }, new List<int>(), DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int>{ 3, 7, 11, 15, 21 }, new List<int>(), DateTime.UtcNow),
            });

        // Act
        var prediction = (await _sut.Predict(lotteryId)).Value!;

        // Assert
        prediction.PredictedNumbers.Should().OnlyContain(n => n >= 1 && n <= config.MainNumbersRange);
    }

    [Test]
    public async Task Given_Valid_Config_When_Predict_Is_Invoked_Should_Return_Unique_MainNumbers()
    {
        // Arrange
        const int lotteryId = 9103;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 5, MainNumbersRange = 30, BonusNumbersCount = 0, BonusNumbersRange = 0 };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(new List<HistoricalDraw>
            {
                new(1, lotteryId, DateTime.UtcNow.AddDays(-2), new List<int>{ 2, 6, 12, 18, 24 }, new List<int>(), DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int>{ 3, 7, 11, 15, 21 }, new List<int>(), DateTime.UtcNow),
            });

        // Act
        var prediction = (await _sut.Predict(lotteryId)).Value!;

        // Assert
        prediction.PredictedNumbers.Should().OnlyHaveUniqueItems();
    }

    [Test]
    public async Task Given_Valid_Config_Without_Bonuses_When_Predict_Is_Invoked_Should_Return_No_BonusNumbers()
    {
        // Arrange
        const int lotteryId = 9104;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 5, MainNumbersRange = 30, BonusNumbersCount = 0, BonusNumbersRange = 0 };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(new List<HistoricalDraw>
            {
                new(1, lotteryId, DateTime.UtcNow.AddDays(-3), new List<int>{ 3, 6, 9, 12, 15 }, new List<int>(), DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int>{ 4, 7, 10, 13, 16 }, new List<int>(), DateTime.UtcNow),
            });

        // Act
        var prediction = (await _sut.Predict(lotteryId)).Value!;

        // Assert
        prediction.BonusNumbers.Should().BeEmpty();
    }

    // ---------- POSITIVE: bonuses ----------

    [Test]
    public async Task Given_Bonus_Config_When_Predict_Is_Invoked_Should_Return_Exact_Bonus_Count()
    {
        // Arrange
        const int lotteryId = 9110;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 5, MainNumbersRange = 35, BonusNumbersCount = 2, BonusNumbersRange = 9 };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(new List<HistoricalDraw>
            {
                new(1, lotteryId, DateTime.UtcNow.AddDays(-3), new List<int>{ 3, 10, 14, 18, 22 }, new List<int>(), DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int>{ 5, 11, 15, 19, 23 }, new List<int>(), DateTime.UtcNow),
            });

        // Act
        var prediction = (await _sut.Predict(lotteryId)).Value!;

        // Assert (random, only check count)
        prediction.BonusNumbers.Count().Should().Be(config.BonusNumbersCount);
    }

    [Test]
    public async Task Given_Bonus_Config_When_Predict_Is_Invoked_Should_Return_Bonuses_In_Range()
    {
        // Arrange
        const int lotteryId = 9110;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 5, MainNumbersRange = 35, BonusNumbersCount = 2, BonusNumbersRange = 9 };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(new List<HistoricalDraw>
            {
                new(1, lotteryId, DateTime.UtcNow.AddDays(-3), new List<int>{ 3, 10, 14, 18, 22 }, new List<int>(), DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int>{ 5, 11, 15, 19, 23 }, new List<int>(), DateTime.UtcNow),
            });

        // Act
        var prediction = (await _sut.Predict(lotteryId)).Value!;

        // Assert (random, only check range)
        prediction.BonusNumbers.Should().OnlyContain(n => n >= 1 && n <= config.BonusNumbersRange);
    }

    [Test]
    public async Task Given_Bonus_Config_When_Predict_Is_Invoked_Should_Return_Unique_Bonuses()
    {
        // Arrange
        const int lotteryId = 9110;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 5, MainNumbersRange = 35, BonusNumbersCount = 2, BonusNumbersRange = 9 };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(new List<HistoricalDraw>
            {
                new(1, lotteryId, DateTime.UtcNow.AddDays(-3), new List<int>{ 3, 10, 14, 18, 22 }, new List<int>(), DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int>{ 5, 11, 15, 19, 23 }, new List<int>(), DateTime.UtcNow),
            });

        // Act
        var prediction = (await _sut.Predict(lotteryId)).Value!;

        // Assert
        prediction.BonusNumbers.Should().OnlyHaveUniqueItems();
    }

    // ---------- POSITIVE: confidence & metadata ----------

    [Test]
    public async Task Given_Valid_Config_When_Predict_Is_Invoked_Should_Return_Confidence_As_Probability()
    {
        // Arrange
        const int lotteryId = 9120;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 5, MainNumbersRange = 35, BonusNumbersCount = 0, BonusNumbersRange = 0 };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(new List<HistoricalDraw>
            {
                new(1, lotteryId, DateTime.UtcNow.AddDays(-3), new List<int>{ 3, 10, 14, 18, 22 }, new List<int>(), DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int>{ 5, 11, 15, 19, 23 }, new List<int>(), DateTime.UtcNow),
            });

        // Act
        var prediction = (await _sut.Predict(lotteryId)).Value!;

        // Assert
        prediction.ConfidenceScore.Should().BeInRange(0.0, 1.0);
    }

    [Test]
    public async Task Given_Valid_Config_When_Predict_Is_Invoked_Should_Set_Strategy_To_NumberSum()
    {
        // Arrange
        const int lotteryId = 9121;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 5, MainNumbersRange = 35, BonusNumbersCount = 0, BonusNumbersRange = 0 };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(new List<HistoricalDraw>
            {
                new(1, lotteryId, DateTime.UtcNow.AddDays(-3), new List<int>{ 3, 10, 14, 18, 22 }, new List<int>(), DateTime.UtcNow)
            });

        // Act
        var prediction = (await _sut.Predict(lotteryId)).Value!;

        // Assert
        prediction.Strategy.Should().Be(PredictionStrategyType.NumberSum);
    }

    // ---------- FAILURE / ERROR CASES ----------

    [Test]
    public async Task Given_No_Active_Configuration_When_Predict_Is_Invoked_Should_Fail()
    {
        // Arrange
        const int lotteryId = 9190;
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(Task.FromResult<LotteryConfigurationDomain?>(null));

        // Act
        var result = await _sut.Predict(lotteryId);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    [Test]
    public async Task Given_No_Active_Configuration_When_Predict_Is_Invoked_Should_Return_Errors()
    {
        // Arrange
        const int lotteryId = 9191;
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(Task.FromResult<LotteryConfigurationDomain?>(null));

        // Act
        var result = await _sut.Predict(lotteryId);

        // Assert
        result.Errors.Should().NotBeNullOrEmpty();
    }

    [Test]
    public async Task Given_No_Active_Configuration_When_Predict_Is_Invoked_Errors_Should_Contain_LotteryId()
    {
        // Arrange
        const int lotteryId = 9192;
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(Task.FromResult<LotteryConfigurationDomain?>(null));

        // Act
        var result = await _sut.Predict(lotteryId);

        // Assert
        result.Errors.Should().Contain(e => e.Contains(lotteryId.ToString()));
    }

    [Test]
    public async Task Given_Empty_History_When_Predict_Is_Invoked_Should_Fail()
    {
        // Arrange
        const int lotteryId = 9193;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 5, MainNumbersRange = 30, BonusNumbersCount = 0, BonusNumbersRange = 0 };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(new List<HistoricalDraw>());

        // Act
        var result = await _sut.Predict(lotteryId);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    [Test]
    public async Task Given_Empty_History_When_Predict_Is_Invoked_Should_Return_Errors()
    {
        // Arrange
        const int lotteryId = 9194;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 5, MainNumbersRange = 30, BonusNumbersCount = 0, BonusNumbersRange = 0 };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(new List<HistoricalDraw>());

        // Act
        var result = await _sut.Predict(lotteryId);

        // Assert
        result.Errors.Should().NotBeNullOrEmpty();
    }

    [Test]
    public async Task Given_Empty_History_When_Predict_Is_Invoked_Errors_Should_Contain_LotteryId()
    {
        // Arrange
        const int lotteryId = 9195;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 5, MainNumbersRange = 30, BonusNumbersCount = 0, BonusNumbersRange = 0 };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(new List<HistoricalDraw>());

        // Act
        var result = await _sut.Predict(lotteryId);

        // Assert
        result.Errors.Should().Contain(e => e.Contains(lotteryId.ToString()));
    }
}