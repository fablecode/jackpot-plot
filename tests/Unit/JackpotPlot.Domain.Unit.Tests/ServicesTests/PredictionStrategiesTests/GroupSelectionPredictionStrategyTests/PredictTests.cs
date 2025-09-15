using FluentAssertions;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Repositories;
using JackpotPlot.Domain.Services.PredictionStrategies;
using NSubstitute;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.ServicesTests.PredictionStrategiesTests.GroupSelectionPredictionStrategyTests;

[TestFixture]
public class PredictTests
{
    private ILotteryConfigurationRepository _configRepo = null!;
    private ILotteryHistoryRepository _historyRepo = null!;
    private GroupSelectionPredictionStrategy _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _configRepo = Substitute.For<ILotteryConfigurationRepository>();
        _historyRepo = Substitute.For<ILotteryHistoryRepository>();
        _sut = new GroupSelectionPredictionStrategy(_configRepo, _historyRepo);
    }

    // ---------- POSITIVE: no bonuses ----------

    [Test]
    public async Task Given_Valid_Config_NoBonuses_When_Predict_Is_Invoked_Should_Succeed()
    {
        // Arrange
        const int lotteryId = 3101;
        var config = new LotteryConfigurationDomain
        {
            LotteryId = lotteryId,
            MainNumbersCount = 5,
            MainNumbersRange = 30,
            BonusNumbersCount = 0,
            BonusNumbersRange = 0
        };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);

        // groups: [1..10], [11..20], [21..30]
        var draws = new List<HistoricalDraw>
            {
                new(1, lotteryId, DateTime.UtcNow.AddDays(-6), new List<int>{ 2, 6, 12, 18, 25 }, new List<int>(), DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-3), new List<int>{ 3, 7, 13, 19, 26 }, new List<int>(), DateTime.UtcNow),
                new(3, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int>{ 4, 9, 15, 20, 30 }, new List<int>(), DateTime.UtcNow),
            };
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(draws);

        // Act
        var result = await _sut.Predict(lotteryId);

        // Assert
        result.IsSuccess.Should().BeTrue(); // success path
    }

    [Test]
    public async Task Given_Valid_Config_NoBonuses_When_Predict_Is_Invoked_Should_Return_Value()
    {
        // Arrange
        const int lotteryId = 3101;
        var config = new LotteryConfigurationDomain
        {
            LotteryId = lotteryId,
            MainNumbersCount = 5,
            MainNumbersRange = 30,
            BonusNumbersCount = 0,
            BonusNumbersRange = 0
        };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(new List<HistoricalDraw>
            {
                new(1, lotteryId, DateTime.UtcNow.AddDays(-6), new List<int>{ 2, 6, 12, 18, 25 }, new List<int>(), DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-3), new List<int>{ 3, 7, 13, 19, 26 }, new List<int>(), DateTime.UtcNow),
                new(3, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int>{ 4, 9, 15, 20, 30 }, new List<int>(), DateTime.UtcNow),
            });

        // Act
        var result = await _sut.Predict(lotteryId);

        // Assert
        result.Value.Should().NotBeNull();
    }

    [Test]
    public async Task Given_Valid_Config_NoBonuses_When_Predict_Is_Invoked_Should_Return_Exactly_Configured_Main_Count()
    {
        // Arrange
        const int lotteryId = 3102;
        var config = new LotteryConfigurationDomain
        {
            LotteryId = lotteryId,
            MainNumbersCount = 4,
            MainNumbersRange = 30,
            BonusNumbersCount = 0,
            BonusNumbersRange = 0
        };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(new List<HistoricalDraw>
            {
                new(1, lotteryId, DateTime.UtcNow.AddDays(-2), new List<int>{ 5, 10, 15, 20 }, new List<int>(), DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int>{ 6, 11, 16, 21 }, new List<int>(), DateTime.UtcNow),
            });

        // Act
        var prediction = (await _sut.Predict(lotteryId)).Value!;

        // Assert
        prediction.PredictedNumbers.Count().Should().Be(config.MainNumbersCount);
    }

    [Test]
    public async Task Given_Valid_Config_NoBonuses_When_Predict_Is_Invoked_Should_Return_MainNumbers_In_Range()
    {
        // Arrange
        const int lotteryId = 3102;
        var config = new LotteryConfigurationDomain
        {
            LotteryId = lotteryId,
            MainNumbersCount = 4,
            MainNumbersRange = 30,
            BonusNumbersCount = 0,
            BonusNumbersRange = 0
        };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(new List<HistoricalDraw>
            {
                new(1, lotteryId, DateTime.UtcNow.AddDays(-2), new List<int>{ 5, 10, 15, 20 }, new List<int>(), DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int>{ 6, 11, 16, 21 }, new List<int>(), DateTime.UtcNow),
            });

        // Act
        var prediction = (await _sut.Predict(lotteryId)).Value!;

        // Assert
        prediction.PredictedNumbers.Should().OnlyContain(n => n >= 1 && n <= config.MainNumbersRange);
    }

    [Test]
    public async Task Given_Valid_Config_NoBonuses_When_Predict_Is_Invoked_Should_Return_Unique_MainNumbers()
    {
        // Arrange
        const int lotteryId = 3103;
        var config = new LotteryConfigurationDomain
        {
            LotteryId = lotteryId,
            MainNumbersCount = 5,
            MainNumbersRange = 30,
            BonusNumbersCount = 0,
            BonusNumbersRange = 0
        };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(new List<HistoricalDraw>
            {
                new(1, lotteryId, DateTime.UtcNow.AddDays(-2), new List<int>{ 5, 10, 15, 20, 25 }, new List<int>(), DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int>{ 6, 11, 16, 21, 26 }, new List<int>(), DateTime.UtcNow),
            });

        // Act
        var prediction = (await _sut.Predict(lotteryId)).Value!;

        // Assert
        prediction.PredictedNumbers.Should().OnlyHaveUniqueItems();
    }

    [Test]
    public async Task Given_Valid_Config_NoBonuses_When_Predict_Is_Invoked_Should_Return_Confidence_As_Probability()
    {
        // Arrange
        const int lotteryId = 3104;
        var config = new LotteryConfigurationDomain
        {
            LotteryId = lotteryId,
            MainNumbersCount = 5,
            MainNumbersRange = 30,
            BonusNumbersCount = 0,
            BonusNumbersRange = 0
        };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(new List<HistoricalDraw>
            {
                new(1, lotteryId, DateTime.UtcNow.AddDays(-3), new List<int>{ 2, 12, 13, 19, 22 }, new List<int>(), DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-2), new List<int>{ 3, 11, 14, 20, 23 }, new List<int>(), DateTime.UtcNow),
                new(3, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int>{ 4, 10, 15, 21, 29 }, new List<int>(), DateTime.UtcNow),
            });

        // Act
        var prediction = (await _sut.Predict(lotteryId)).Value!;

        // Assert
        prediction.ConfidenceScore.Should().BeGreaterThan(0.0).And.BeLessOrEqualTo(1.0);
    }

    // ---------- POSITIVE: with bonuses ----------

    [Test]
    public async Task Given_Valid_Config_With_Bonuses_When_Predict_Is_Invoked_Should_Return_Exact_Bonus_Count()
    {
        // Arrange
        const int lotteryId = 3110;
        var config = new LotteryConfigurationDomain
        {
            LotteryId = lotteryId,
            MainNumbersCount = 5,
            MainNumbersRange = 35,
            BonusNumbersCount = 2,
            BonusNumbersRange = 9
        };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(new List<HistoricalDraw>
            {
                new(1, lotteryId, DateTime.UtcNow.AddDays(-2), new List<int>{ 4, 12, 18, 22, 30 }, new List<int>{ 3 }, DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int>{ 5, 13, 19, 23, 31 }, new List<int>{ 4 }, DateTime.UtcNow),
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
        const int lotteryId = 3110;
        var config = new LotteryConfigurationDomain
        {
            LotteryId = lotteryId,
            MainNumbersCount = 5,
            MainNumbersRange = 35,
            BonusNumbersCount = 2,
            BonusNumbersRange = 9
        };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(new List<HistoricalDraw>
            {
                new(1, lotteryId, DateTime.UtcNow.AddDays(-2), new List<int>{ 4, 12, 18, 22, 30 }, new List<int>{ 3 }, DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int>{ 5, 13, 19, 23, 31 }, new List<int>{ 4 }, DateTime.UtcNow),
            });

        // Act
        var prediction = (await _sut.Predict(lotteryId)).Value!;

        // Assert
        prediction.BonusNumbers.Should().OnlyContain(n => n >= 1 && n <= config.BonusNumbersRange);
    }

    [Test]
    public async Task Given_Valid_Config_With_Bonuses_When_Predict_Is_Invoked_Should_Return_Unique_BonusNumbers()
    {
        // Arrange
        const int lotteryId = 3110;
        var config = new LotteryConfigurationDomain
        {
            LotteryId = lotteryId,
            MainNumbersCount = 5,
            MainNumbersRange = 35,
            BonusNumbersCount = 2,
            BonusNumbersRange = 9
        };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(new List<HistoricalDraw>
            {
                new(1, lotteryId, DateTime.UtcNow.AddDays(-2), new List<int>{ 4, 12, 18, 22, 30 }, new List<int>{ 3 }, DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int>{ 5, 13, 19, 23, 31 }, new List<int>{ 4 }, DateTime.UtcNow),
            });

        // Act
        var prediction = (await _sut.Predict(lotteryId)).Value!;

        // Assert
        prediction.BonusNumbers.Should().OnlyHaveUniqueItems();
    }

    // ---------- FAILURE / ERROR CASES ----------

    [Test]
    public async Task Given_No_Active_Configuration_When_Predict_Is_Invoked_Should_Fail()
    {
        // Arrange
        const int lotteryId = 3990;
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
        const int lotteryId = 3991;
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
        const int lotteryId = 3992;
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
        const int lotteryId = 3993;
        var config = new LotteryConfigurationDomain
        {
            LotteryId = lotteryId,
            MainNumbersCount = 5,
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
        const int lotteryId = 3994;
        var config = new LotteryConfigurationDomain
        {
            LotteryId = lotteryId,
            MainNumbersCount = 5,
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
        const int lotteryId = 3995;
        var config = new LotteryConfigurationDomain
        {
            LotteryId = lotteryId,
            MainNumbersCount = 5,
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