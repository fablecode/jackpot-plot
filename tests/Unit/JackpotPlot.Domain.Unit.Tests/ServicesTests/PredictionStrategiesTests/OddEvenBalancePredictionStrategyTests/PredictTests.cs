using FluentAssertions;
using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Repositories;
using JackpotPlot.Domain.Services.PredictionStrategies;
using NSubstitute;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.ServicesTests.PredictionStrategiesTests.OddEvenBalancePredictionStrategyTests;

[TestFixture]
public class PredictTests
{
    private ILotteryConfigurationRepository _configRepo = null!;
    private ILotteryHistoryRepository _historyRepo = null!;
    private OddEvenBalancePredictionStrategy _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _configRepo = Substitute.For<ILotteryConfigurationRepository>();
        _historyRepo = Substitute.For<ILotteryHistoryRepository>();
        _sut = new OddEvenBalancePredictionStrategy(_configRepo, _historyRepo);
    }

    // ---------- POSITIVE: base success ----------

    [Test]
    public async Task Given_Valid_Config_And_History_When_Predict_Is_Invoked_Should_Succeed()
    {
        // Arrange
        const int lotteryId = 9201;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 4, MainNumbersRange = 20, BonusNumbersCount = 0, BonusNumbersRange = 0 };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);

        // Each draw: 3 odd + 1 even -> overall oddRatio = 6/8 = 0.75
        var draws = new List<HistoricalDraw>
            {
                new(1, lotteryId, DateTime.UtcNow.AddDays(-2), new List<int>{ 1, 3, 5, 6 }, new List<int>(), DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int>{ 7, 9, 11, 10 }, new List<int>(), DateTime.UtcNow),
            };
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(draws);

        // Act
        var result = await _sut.Predict(lotteryId);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task Given_Valid_Config_And_History_When_Predict_Is_Invoked_Should_Return_Value()
    {
        // Arrange
        const int lotteryId = 9201;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 4, MainNumbersRange = 20, BonusNumbersCount = 0, BonusNumbersRange = 0 };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(new List<HistoricalDraw>
            {
                new(1, lotteryId, DateTime.UtcNow.AddDays(-2), new List<int>{ 1, 3, 5, 6 }, new List<int>(), DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int>{ 7, 9, 11, 10 }, new List<int>(), DateTime.UtcNow),
            });

        // Act
        var result = await _sut.Predict(lotteryId);

        // Assert
        result.Value.Should().NotBeNull();
    }

    // ---------- POSITIVE: counts, ranges, uniqueness ----------

    [Test]
    public async Task Given_Valid_Config_When_Predict_Is_Invoked_Should_Return_Exactly_Configured_Main_Count()
    {
        // Arrange
        const int lotteryId = 9202;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 6, MainNumbersRange = 40, BonusNumbersCount = 0, BonusNumbersRange = 0 };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);

        var draws = new List<HistoricalDraw>
            {
                new(1, lotteryId, DateTime.UtcNow.AddDays(-3), new List<int>{ 1,3,5,2,4,6 }, new List<int>(), DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int>{ 7,9,11,8,10,12 }, new List<int>(), DateTime.UtcNow),
            };
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(draws);

        // Act
        var prediction = (await _sut.Predict(lotteryId)).Value!;

        // Assert
        prediction.PredictedNumbers.Count().Should().Be(config.MainNumbersCount);
    }

    [Test]
    public async Task Given_Valid_Config_When_Predict_Is_Invoked_Should_Return_MainNumbers_In_Range()
    {
        // Arrange
        const int lotteryId = 9202;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 6, MainNumbersRange = 40, BonusNumbersCount = 0, BonusNumbersRange = 0 };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(new List<HistoricalDraw>
            {
                new(1, lotteryId, DateTime.UtcNow.AddDays(-3), new List<int>{ 1,3,5,2,4,6 }, new List<int>(), DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int>{ 7,9,11,8,10,12 }, new List<int>(), DateTime.UtcNow),
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
        const int lotteryId = 9203;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 6, MainNumbersRange = 40, BonusNumbersCount = 0, BonusNumbersRange = 0 };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);

        _historyRepo.GetHistoricalDraws(lotteryId).Returns(new List<HistoricalDraw>
            {
                new(1, lotteryId, DateTime.UtcNow.AddDays(-3), new List<int>{ 1,3,5,2,4,6 }, new List<int>(), DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int>{ 7,9,11,8,10,12 }, new List<int>(), DateTime.UtcNow),
            });

        // Act
        var prediction = (await _sut.Predict(lotteryId)).Value!;

        // Assert
        prediction.PredictedNumbers.Should().OnlyHaveUniqueItems();
    }

    // ---------- POSITIVE: odd/even split & confidence ----------

    [Test]
    public async Task Given_LowEvenRatio_When_Predict_Is_Invoked_Should_Respect_Rounded_Odd_Count()
    {
        // Arrange
        // Each draw: 3 odd + 1 even -> oddRatio=0.75; MainNumbersCount=4 -> oddCount=3
        const int lotteryId = 9204;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 4, MainNumbersRange = 20, BonusNumbersCount = 0, BonusNumbersRange = 0 };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);

        var draws = new List<HistoricalDraw>
            {
                new(1, lotteryId, DateTime.UtcNow.AddDays(-2), new List<int>{ 1,3,5,6 }, new List<int>(), DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int>{ 7,9,11,10 }, new List<int>(), DateTime.UtcNow),
            };
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(draws);

        // Act
        var pred = (await _sut.Predict(lotteryId)).Value!;
        var midOddCount = pred.PredictedNumbers.Count(n => n % 2 != 0);

        // Assert
        midOddCount.Should().Be(3);
    }

    [Test]
    public async Task Given_Draws_With_Consistent_OddEven_Split_When_Predict_Is_Invoked_Should_Return_Confidence_Of_One()
    {
        // Arrange
        // Both draws are 3 odd + 1 even, predicted is also 3/1 -> matches all draws
        const int lotteryId = 9205;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 4, MainNumbersRange = 20, BonusNumbersCount = 0, BonusNumbersRange = 0 };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);

        var draws = new List<HistoricalDraw>
            {
                new(1, lotteryId, DateTime.UtcNow.AddDays(-2), new List<int>{ 1,3,5,6 }, new List<int>(), DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int>{ 7,9,11,10 }, new List<int>(), DateTime.UtcNow),
            };
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(draws);

        // Act
        var pred = (await _sut.Predict(lotteryId)).Value!;

        // Assert
        pred.ConfidenceScore.Should().BeApproximately(1.0, 1e-12);
    }

    [Test]
    public async Task Given_Valid_Config_When_Predict_Is_Invoked_Should_Return_Confidence_In_Range()
    {
        // Arrange
        const int lotteryId = 9206;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 6, MainNumbersRange = 40, BonusNumbersCount = 0, BonusNumbersRange = 0 };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(new List<HistoricalDraw>
            {
                // mixed odd/even counts
                new(1, lotteryId, DateTime.UtcNow.AddDays(-3), new List<int>{ 1,2,3,4,5,6 }, new List<int>(), DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int>{ 7,8,10,12,14,15 }, new List<int>(), DateTime.UtcNow),
            });

        // Act
        var pred = (await _sut.Predict(lotteryId)).Value!;

        // Assert
        pred.ConfidenceScore.Should().BeInRange(0.0, 1.0);
    }

    // ---------- POSITIVE: bonuses ----------

    [Test]
    public async Task Given_Bonus_Config_When_Predict_Is_Invoked_Should_Return_Exact_Bonus_Count()
    {
        // Arrange
        const int lotteryId = 9210;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 6, MainNumbersRange = 40, BonusNumbersCount = 2, BonusNumbersRange = 9 };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);

        _historyRepo.GetHistoricalDraws(lotteryId).Returns(new List<HistoricalDraw>
            {
                // content doesn't affect bonus generation
                new(1, lotteryId, DateTime.UtcNow.AddDays(-2), new List<int>{ 1,3,5,6 }, new List<int>(), DateTime.UtcNow),
            });

        // Act
        var pred = (await _sut.Predict(lotteryId)).Value!;

        // Assert
        pred.BonusNumbers.Count().Should().Be(config.BonusNumbersCount);
    }

    [Test]
    public async Task Given_Bonus_Config_When_Predict_Is_Invoked_Should_Return_Bonuses_In_Range()
    {
        // Arrange
        const int lotteryId = 9210;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 6, MainNumbersRange = 40, BonusNumbersCount = 2, BonusNumbersRange = 9 };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);

        _historyRepo.GetHistoricalDraws(lotteryId).Returns(new List<HistoricalDraw>
            {
                new(1, lotteryId, DateTime.UtcNow.AddDays(-2), new List<int>{ 1,3,5,6 }, new List<int>(), DateTime.UtcNow),
            });

        // Act
        var pred = (await _sut.Predict(lotteryId)).Value!;

        // Assert
        pred.BonusNumbers.Should().OnlyContain(n => n >= 1 && n <= config.BonusNumbersRange);
    }

    [Test]
    public async Task Given_Bonus_Config_When_Predict_Is_Invoked_Should_Return_Unique_Bonuses()
    {
        // Arrange
        const int lotteryId = 9210;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 6, MainNumbersRange = 40, BonusNumbersCount = 2, BonusNumbersRange = 9 };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);

        _historyRepo.GetHistoricalDraws(lotteryId).Returns(new List<HistoricalDraw>
            {
                new(1, lotteryId, DateTime.UtcNow.AddDays(-2), new List<int>{ 1,3,5,6 }, new List<int>(), DateTime.UtcNow),
            });

        // Act
        var pred = (await _sut.Predict(lotteryId)).Value!;

        // Assert
        pred.BonusNumbers.Should().OnlyHaveUniqueItems();
    }

    // ---------- POSITIVE: metadata ----------

    [Test]
    public async Task Given_Valid_Config_When_Predict_Is_Invoked_Should_Set_Strategy_Name_As_In_Code()
    {
        // Arrange
        // Note: The strategy string is set to StatisticalAveraging in the implementation.
        const int lotteryId = 9220;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 4, MainNumbersRange = 20, BonusNumbersCount = 0, BonusNumbersRange = 0 };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(new List<HistoricalDraw>
            {
                new(1, lotteryId, DateTime.UtcNow.AddDays(-2), new List<int>{ 1,3,5,6 }, new List<int>(), DateTime.UtcNow),
            });

        // Act
        var pred = (await _sut.Predict(lotteryId)).Value!;

        // Assert
        pred.Strategy.Should().Be(PredictionStrategyType.StatisticalAveraging);
    }

    // ---------- FAILURE / ERROR CASES ----------

    [Test]
    public async Task Given_No_Active_Configuration_When_Predict_Is_Invoked_Should_Fail()
    {
        // Arrange
        const int lotteryId = 9290;
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
        const int lotteryId = 9291;
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
        const int lotteryId = 9292;
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
        const int lotteryId = 9293;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 6, MainNumbersRange = 40, BonusNumbersCount = 0, BonusNumbersRange = 0 };
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
        const int lotteryId = 9294;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 6, MainNumbersRange = 40, BonusNumbersCount = 0, BonusNumbersRange = 0 };
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
        const int lotteryId = 9295;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 6, MainNumbersRange = 40, BonusNumbersCount = 0, BonusNumbersRange = 0 };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(new List<HistoricalDraw>());

        // Act
        var result = await _sut.Predict(lotteryId);

        // Assert
        result.Errors.Should().Contain(e => e.Contains(lotteryId.ToString()));
    }
}