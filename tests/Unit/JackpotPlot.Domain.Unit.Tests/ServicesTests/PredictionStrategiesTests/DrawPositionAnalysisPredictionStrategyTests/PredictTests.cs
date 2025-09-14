using FluentAssertions;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Repositories;
using JackpotPlot.Domain.Services.PredictionStrategies;
using NSubstitute;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.ServicesTests.PredictionStrategiesTests.DrawPositionAnalysisPredictionStrategyTests;

[TestFixture]
public class PredictTests
{
    private ILotteryConfigurationRepository _configRepo = null!;
    private ILotteryHistoryRepository _historyRepo = null!;
    private DrawPositionAnalysisPredictionStrategy _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _configRepo = Substitute.For<ILotteryConfigurationRepository>();
        _historyRepo = Substitute.For<ILotteryHistoryRepository>();
        _sut = new DrawPositionAnalysisPredictionStrategy(_configRepo, _historyRepo);
    }

    // ---------- POSITIVE-STYLE ASSERTIONS ----------

    [Test]
    public async Task Given_Valid_Config_Without_Bonuses_When_Predict_Is_Invoked_Should_Succeed()
    {
        // Arrange
        const int lotteryId = 8001;
        var config = new LotteryConfigurationDomain
        {
            LotteryId = lotteryId,
            MainNumbersCount = 3,
            MainNumbersRange = 50,
            BonusNumbersCount = 0,
            BonusNumbersRange = 0
        };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);

        // Make the most frequent per-position numbers unambiguous (no ties).
        var draws = new List<HistoricalDraw>
            {
                new(1, lotteryId, DateTime.UtcNow.AddDays(-3), new List<int>{ 1, 5, 9 }, new List<int>(), DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-2), new List<int>{ 1, 5, 9 }, new List<int>(), DateTime.UtcNow),
                new(3, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int>{ 1, 5, 9 }, new List<int>(), DateTime.UtcNow),
            };
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(draws);

        // Act
        var result = await _sut.Predict(lotteryId);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task Given_Valid_Config_Without_Bonuses_When_Predict_Is_Invoked_Should_Return_Exactly_MainNumbersCount_When_Positions_Are_Unique()
    {
        // Arrange
        const int lotteryId = 8002;
        var config = new LotteryConfigurationDomain
        {
            LotteryId = lotteryId,
            MainNumbersCount = 3,
            MainNumbersRange = 20,
            BonusNumbersCount = 0,
            BonusNumbersRange = 0
        };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);

        var draws = new List<HistoricalDraw>
            {
                new(1, lotteryId, DateTime.UtcNow.AddDays(-3), new List<int>{ 2, 7, 11 }, new List<int>(), DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-2), new List<int>{ 2, 7, 11 }, new List<int>(), DateTime.UtcNow),
                new(3, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int>{ 2, 7, 11 }, new List<int>(), DateTime.UtcNow),
            };
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(draws);

        // Act
        var prediction = (await _sut.Predict(lotteryId)).Value!;

        // Assert
        prediction.PredictedNumbers.Count().Should().Be(config.MainNumbersCount);
    }

    [Test]
    public async Task Given_Valid_Config_With_Bonuses_When_Predict_Is_Invoked_Should_Return_Exact_Bonus_Count()
    {
        // Arrange
        const int lotteryId = 8003;
        var config = new LotteryConfigurationDomain
        {
            LotteryId = lotteryId,
            MainNumbersCount = 3,
            MainNumbersRange = 20,
            BonusNumbersCount = 2,
            BonusNumbersRange = 9
        };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);

        var draws = new List<HistoricalDraw>
            {
                new(1, lotteryId, DateTime.UtcNow.AddDays(-3), new List<int>{ 1, 5, 9 }, new List<int>(), DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-2), new List<int>{ 1, 5, 9 }, new List<int>(), DateTime.UtcNow),
            };
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(draws);

        // Act
        var prediction = (await _sut.Predict(lotteryId)).Value!;

        // Assert
        prediction.BonusNumbers.Count().Should().Be(config.BonusNumbersCount);
    }

    [Test]
    public async Task Given_Valid_Config_With_Bonuses_When_Predict_Is_Invoked_Should_Return_Exact_Bonus_Count_Within_Range()
    {
        // Arrange
        const int lotteryId = 8003;
        var config = new LotteryConfigurationDomain
        {
            LotteryId = lotteryId,
            MainNumbersCount = 3,
            MainNumbersRange = 20,
            BonusNumbersCount = 2,
            BonusNumbersRange = 9
        };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);

        var draws = new List<HistoricalDraw>
            {
                new(1, lotteryId, DateTime.UtcNow.AddDays(-3), new List<int>{ 1, 5, 9 }, new List<int>(), DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-2), new List<int>{ 1, 5, 9 }, new List<int>(), DateTime.UtcNow),
            };
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(draws);

        // Act
        var prediction = (await _sut.Predict(lotteryId)).Value!;

        // Assert
        prediction.BonusNumbers.Should().OnlyContain(n => n >= 1 && n <= config.BonusNumbersRange);
    }

    [Test]
    public async Task Given_Valid_Config_When_Predict_Is_Invoked_Should_Return_MainNumbers_Within_Configured_Range()
    {
        // Arrange
        const int lotteryId = 8004;
        var config = new LotteryConfigurationDomain
        {
            LotteryId = lotteryId,
            MainNumbersCount = 4,
            MainNumbersRange = 40,
            BonusNumbersCount = 0,
            BonusNumbersRange = 0
        };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);

        // Make position frequencies clear; order in results is shuffled by the strategy.
        var draws = new List<HistoricalDraw>
            {
                new(1, lotteryId, DateTime.UtcNow.AddDays(-3), new List<int>{ 3, 8, 13, 21 }, new List<int>(), DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-2), new List<int>{ 3, 8, 13, 21 }, new List<int>(), DateTime.UtcNow),
            };
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(draws);

        // Act
        var prediction = (await _sut.Predict(lotteryId)).Value!;

        // Assert
        prediction.PredictedNumbers.Should().OnlyContain(n => n >= 1 && n <= config.MainNumbersRange);
    }

    [Test]
    public async Task Given_Valid_Config_When_Predict_Is_Invoked_Should_Return_MainNumbers_Within_Probability_Confidence()
    {
        // Arrange
        const int lotteryId = 8004;
        var config = new LotteryConfigurationDomain
        {
            LotteryId = lotteryId,
            MainNumbersCount = 4,
            MainNumbersRange = 40,
            BonusNumbersCount = 0,
            BonusNumbersRange = 0
        };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);

        // Make position frequencies clear; order in results is shuffled by the strategy.
        var draws = new List<HistoricalDraw>
            {
                new(1, lotteryId, DateTime.UtcNow.AddDays(-3), new List<int>{ 3, 8, 13, 21 }, new List<int>(), DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-2), new List<int>{ 3, 8, 13, 21 }, new List<int>(), DateTime.UtcNow),
            };
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(draws);

        // Act
        var prediction = (await _sut.Predict(lotteryId)).Value!;

        // Assert
        prediction.ConfidenceScore.Should().BeInRange(0.0, 1.0); // order is randomized; only assert probability bounds
    }

    // ---------- NEGATIVE-STYLE ASSERTIONS (invariants phrased as "should not") ----------

    [Test]
    public async Task Given_Valid_Config_When_Predict_Is_Invoked_Should_Not_Exceed_MainNumbersCount()
    {
        // Arrange
        const int lotteryId = 8005;
        var config = new LotteryConfigurationDomain
        {
            LotteryId = lotteryId,
            MainNumbersCount = 5,
            MainNumbersRange = 30,
            BonusNumbersCount = 0,
            BonusNumbersRange = 0
        };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);

        var draws = new List<HistoricalDraw>
            {
                new(1, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int>{ 4, 7, 12, 18, 24 }, new List<int>(), DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-2), new List<int>{ 4, 7, 12, 18, 24 }, new List<int>(), DateTime.UtcNow),
            };
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(draws);

        // Act
        var prediction = (await _sut.Predict(lotteryId)).Value!;

        // Assert
        prediction.PredictedNumbers.Count().Should().BeLessOrEqualTo(config.MainNumbersCount);
    }

    [Test]
    public async Task Given_Valid_Config_When_Predict_Is_Invoked_Should_Not_Return_Main_Numbers_Outside_Configured_Range()
    {
        // Arrange
        const int lotteryId = 8006;
        var config = new LotteryConfigurationDomain
        {
            LotteryId = lotteryId,
            MainNumbersCount = 4,
            MainNumbersRange = 25,
            BonusNumbersCount = 2,
            BonusNumbersRange = 10
        };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);

        var draws = new List<HistoricalDraw>
            {
                new(1, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int>{ 2, 6, 10, 14 }, new List<int>(), DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-2), new List<int>{ 2, 6, 10, 14 }, new List<int>(), DateTime.UtcNow),
            };
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(draws);

        // Act
        var prediction = (await _sut.Predict(lotteryId)).Value!;

        // Assert
        prediction.PredictedNumbers.Should().OnlyContain(n => n >= 1 && n <= config.MainNumbersRange);
    }

    [Test]
    public async Task Given_Valid_Config_When_Predict_Is_Invoked_Should_Not_Return_Bonus_Numbers_Outside_Configured_Range()
    {
        // Arrange
        const int lotteryId = 8006;
        var config = new LotteryConfigurationDomain
        {
            LotteryId = lotteryId,
            MainNumbersCount = 4,
            MainNumbersRange = 25,
            BonusNumbersCount = 2,
            BonusNumbersRange = 10
        };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);

        var draws = new List<HistoricalDraw>
            {
                new(1, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int>{ 2, 6, 10, 14 }, new List<int>(), DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-2), new List<int>{ 2, 6, 10, 14 }, new List<int>(), DateTime.UtcNow),
            };
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(draws);

        // Act
        var prediction = (await _sut.Predict(lotteryId)).Value!;

        // Assert
        prediction.BonusNumbers.Should().OnlyContain(n => n >= 1 && n <= config.BonusNumbersRange);
    }

    // ---------- FAILURE / ERROR NEGATIVE CASES ----------

    [Test]
    public async Task Given_No_Active_Configuration_When_Predict_Is_Invoked_Should_Fail()
    {
        // Arrange
        const int lotteryId = 8991;
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
        const int lotteryId = 8991;
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
        const int lotteryId = 8991;
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
        const int lotteryId = 8992;
        var config = new LotteryConfigurationDomain
        {
            LotteryId = lotteryId,
            MainNumbersCount = 3,
            MainNumbersRange = 30,
            BonusNumbersCount = 0,
            BonusNumbersRange = 0
        };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(Task.FromResult<ICollection<HistoricalDraw>>(new List<HistoricalDraw>()));

        // Act
        var result = await _sut.Predict(lotteryId);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    [Test]
    public async Task Given_Empty_History_When_Predict_Is_Invoked_Should_Return_Errors()
    {
        // Arrange
        const int lotteryId = 8992;
        var config = new LotteryConfigurationDomain
        {
            LotteryId = lotteryId,
            MainNumbersCount = 3,
            MainNumbersRange = 30,
            BonusNumbersCount = 0,
            BonusNumbersRange = 0
        };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(Task.FromResult<ICollection<HistoricalDraw>>(new List<HistoricalDraw>()));

        // Act
        var result = await _sut.Predict(lotteryId);

        // Assert
        result.Errors.Should().NotBeNullOrEmpty();
    }

    [Test]
    public async Task Given_Empty_History_When_Predict_Is_Invoked_Errors_Should_Contain_LotteryId()
    {
        // Arrange
        const int lotteryId = 8992;
        var config = new LotteryConfigurationDomain
        {
            LotteryId = lotteryId,
            MainNumbersCount = 3,
            MainNumbersRange = 30,
            BonusNumbersCount = 0,
            BonusNumbersRange = 0
        };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(Task.FromResult<ICollection<HistoricalDraw>>(new List<HistoricalDraw>()));

        // Act
        var result = await _sut.Predict(lotteryId);

        // Assert
        result.Errors.Should().Contain(e => e.Contains(lotteryId.ToString()));
    }
}