using FluentAssertions;
using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Repositories;
using JackpotPlot.Domain.Services.PredictionStrategies;
using NSubstitute;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.ServicesTests.PredictionStrategiesTests.NumberChainPredictionStrategyTests;

[TestFixture]
public class PredictTests
{
    private ILotteryConfigurationRepository _configRepo = null!;
    private ILotteryHistoryRepository _historyRepo = null!;
    private NumberChainPredictionStrategy _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _configRepo = Substitute.For<ILotteryConfigurationRepository>();
        _historyRepo = Substitute.For<ILotteryHistoryRepository>();
        _sut = new NumberChainPredictionStrategy(_configRepo, _historyRepo);
    }

    // ---------- POSITIVE: base success ----------

    [Test]
    public async Task Given_Valid_Config_And_History_When_Predict_Is_Invoked_Should_Succeed()
    {
        // Arrange
        const int lotteryId = 80001;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 3, MainNumbersRange = 20, BonusNumbersCount = 0, BonusNumbersRange = 0 };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);

        var draws = new List<HistoricalDraw>
            {
                new(1, lotteryId, DateTime.UtcNow.AddDays(-3), new List<int>{ 1, 2, 3 }, new List<int>(), DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int>{ 2, 3, 4 }, new List<int>(), DateTime.UtcNow),
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
        const int lotteryId = 80001;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 3, MainNumbersRange = 20, BonusNumbersCount = 0, BonusNumbersRange = 0 };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(new List<HistoricalDraw>
            {
                new(1, lotteryId, DateTime.UtcNow.AddDays(-3), new List<int>{ 1, 2, 3 }, new List<int>(), DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int>{ 2, 3, 4 }, new List<int>(), DateTime.UtcNow),
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
        const int lotteryId = 80002;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 4, MainNumbersRange = 40, BonusNumbersCount = 0, BonusNumbersRange = 0 };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(new List<HistoricalDraw>
            {
                new(1, lotteryId, DateTime.UtcNow.AddDays(-2), new List<int>{ 5, 6, 7, 8 }, new List<int>(), DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int>{ 6, 7, 8, 9 }, new List<int>(), DateTime.UtcNow),
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
        const int lotteryId = 80002;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 4, MainNumbersRange = 40, BonusNumbersCount = 0, BonusNumbersRange = 0 };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(new List<HistoricalDraw>
            {
                new(1, lotteryId, DateTime.UtcNow.AddDays(-2), new List<int>{ 5, 6, 7, 8 }, new List<int>(), DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int>{ 6, 7, 8, 9 }, new List<int>(), DateTime.UtcNow),
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
        const int lotteryId = 80003;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 5, MainNumbersRange = 50, BonusNumbersCount = 0, BonusNumbersRange = 0 };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(new List<HistoricalDraw>
            {
                new(1, lotteryId, DateTime.UtcNow.AddDays(-2), new List<int>{ 2, 4, 6, 8, 10 }, new List<int>(), DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int>{ 3, 5, 7, 9, 11 }, new List<int>(), DateTime.UtcNow),
            });

        // Act
        var prediction = (await _sut.Predict(lotteryId)).Value!;

        // Assert
        prediction.PredictedNumbers.Should().OnlyHaveUniqueItems();
    }

    // ---------- POSITIVE: deterministic chain selection ----------

    [Test]
    public async Task Given_Repeated_Triplet_History_When_Predict_Is_Invoked_Should_Select_That_Triplet()
    {
        // Arrange
        // Two draws both [1,2,3] => top chains comprise {1,2,3} & all its pairs -> selected numbers = {1,2,3} (order shuffled)
        const int lotteryId = 80010;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 3, MainNumbersRange = 20, BonusNumbersCount = 0, BonusNumbersRange = 0 };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);

        var draws = new List<HistoricalDraw>
            {
                new(1, lotteryId, DateTime.UtcNow.AddDays(-2), new List<int>{ 1, 2, 3 }, new List<int>(), DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int>{ 1, 2, 3 }, new List<int>(), DateTime.UtcNow),
            };
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(draws);

        // Act
        var prediction = (await _sut.Predict(lotteryId)).Value!;

        // Assert (order-independent due to shuffle)
        prediction.PredictedNumbers.Should().BeEquivalentTo(new[] { 1, 2, 3 });
    }

    [Test]
    public async Task Given_Repeated_Triplet_History_When_Predict_Is_Invoked_Should_Compute_Confidence_As_One()
    {
        // Arrange
        // With both draws [1,2,3] and predicted {1,2,3}:
        // chainConfidence = 4/4 (three pairs + one triplet), numberConfidence = 6/6 -> avg = 1.0
        const int lotteryId = 80010;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 3, MainNumbersRange = 20, BonusNumbersCount = 0, BonusNumbersRange = 0 };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(new List<HistoricalDraw>
            {
                new(1, lotteryId, DateTime.UtcNow.AddDays(-2), new List<int>{ 1, 2, 3 }, new List<int>(), DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int>{ 1, 2, 3 }, new List<int>(), DateTime.UtcNow),
            });

        // Act
        var prediction = (await _sut.Predict(lotteryId)).Value!;

        // Assert
        prediction.ConfidenceScore.Should().BeApproximately(1.0, 1e-12);
    }

    // ---------- POSITIVE: bonuses ----------

    [Test]
    public async Task Given_Bonus_Config_When_Predict_Is_Invoked_Should_Return_Exact_Bonus_Count()
    {
        // Arrange
        const int lotteryId = 80020;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 3, MainNumbersRange = 20, BonusNumbersCount = 2, BonusNumbersRange = 9 };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(new List<HistoricalDraw>
            {
                new(1, lotteryId, DateTime.UtcNow.AddDays(-3), new List<int>{ 2, 4, 6 }, new List<int>(), DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int>{ 3, 5, 7 }, new List<int>(), DateTime.UtcNow),
            });

        // Act
        var prediction = (await _sut.Predict(lotteryId)).Value!;

        // Assert (random bonus values, only check count)
        prediction.BonusNumbers.Count().Should().Be(config.BonusNumbersCount);
    }

    [Test]
    public async Task Given_Bonus_Config_When_Predict_Is_Invoked_Should_Return_Bonuses_In_Range()
    {
        // Arrange
        const int lotteryId = 80020;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 3, MainNumbersRange = 20, BonusNumbersCount = 2, BonusNumbersRange = 9 };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(new List<HistoricalDraw>
            {
                new(1, lotteryId, DateTime.UtcNow.AddDays(-3), new List<int>{ 2, 4, 6 }, new List<int>(), DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int>{ 3, 5, 7 }, new List<int>(), DateTime.UtcNow),
            });

        // Act
        var prediction = (await _sut.Predict(lotteryId)).Value!;

        // Assert (random bonus values, only check range)
        prediction.BonusNumbers.Should().OnlyContain(n => n >= 1 && n <= config.BonusNumbersRange);
    }

    [Test]
    public async Task Given_Bonus_Config_When_Predict_Is_Invoked_Should_Return_Unique_Bonuses()
    {
        // Arrange
        const int lotteryId = 80020;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 3, MainNumbersRange = 20, BonusNumbersCount = 2, BonusNumbersRange = 9 };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(new List<HistoricalDraw>
            {
                new(1, lotteryId, DateTime.UtcNow.AddDays(-3), new List<int>{ 2, 4, 6 }, new List<int>(), DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int>{ 3, 5, 7 }, new List<int>(), DateTime.UtcNow),
            });

        // Act
        var prediction = (await _sut.Predict(lotteryId)).Value!;

        // Assert
        prediction.BonusNumbers.Should().OnlyHaveUniqueItems();
    }

    // ---------- POSITIVE: metadata ----------

    [Test]
    public async Task Given_Valid_Config_When_Predict_Is_Invoked_Should_Set_Strategy_To_NumberChain()
    {
        // Arrange
        const int lotteryId = 80030;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 3, MainNumbersRange = 20, BonusNumbersCount = 0, BonusNumbersRange = 0 };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(new List<HistoricalDraw>
            {
                new(1, lotteryId, DateTime.UtcNow.AddDays(-2), new List<int>{ 1, 2, 3 }, new List<int>(), DateTime.UtcNow)
            });

        // Act
        var prediction = (await _sut.Predict(lotteryId)).Value!;

        // Assert
        prediction.Strategy.Should().Be(PredictionStrategyType.NumberChain);
    }

    // ---------- FAILURE / ERROR CASES ----------

    [Test]
    public async Task Given_No_Active_Configuration_When_Predict_Is_Invoked_Should_Fail()
    {
        // Arrange
        const int lotteryId = 80990;
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(Task.FromResult<LotteryConfigurationDomain?>(null));

        // Act
        var result = await _sut.Predict(lotteryId);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    [Test]
    public async Task Given_No_Active_Configuration_When_Predict_Is_Invoked_Errors_Should_Contain_LotteryId()
    {
        // Arrange
        const int lotteryId = 80991;
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
        const int lotteryId = 80992;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 3, MainNumbersRange = 20, BonusNumbersCount = 0, BonusNumbersRange = 0 };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(new List<HistoricalDraw>());

        // Act
        var result = await _sut.Predict(lotteryId);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    [Test]
    public async Task Given_Empty_History_When_Predict_Is_Invoked_Errors_Should_Contain_LotteryId()
    {
        // Arrange
        const int lotteryId = 80993;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 3, MainNumbersRange = 20, BonusNumbersCount = 0, BonusNumbersRange = 0 };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(new List<HistoricalDraw>());

        // Act
        var result = await _sut.Predict(lotteryId);

        // Assert
        result.Errors.Should().Contain(e => e.Contains(lotteryId.ToString()));
    }
}