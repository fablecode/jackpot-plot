using FluentAssertions;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Repositories;
using JackpotPlot.Domain.Services.PredictionStrategies;
using NSubstitute;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.ServicesTests.PredictionStrategiesTests.FrequencyPredictionStrategyTests;

[TestFixture]
public class PredictTests
{
    private ILotteryConfigurationRepository _configRepo = null!;
    private ILotteryHistoryRepository _historyRepo = null!;
    private FrequencyPredictionStrategy _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _configRepo = Substitute.For<ILotteryConfigurationRepository>();
        _historyRepo = Substitute.For<ILotteryHistoryRepository>();
        _sut = new FrequencyPredictionStrategy(_configRepo, _historyRepo);
    }

    // ---------- POSITIVE-STYLE ASSERTIONS ----------

    [Test]
    public async Task Given_Valid_Config_Without_Bonuses_When_Predict_Is_Invoked_Should_Succeed()
    {
        // Arrange
        const int lotteryId = 9001;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 3, MainNumbersRange = 30, BonusNumbersCount = 0, BonusNumbersRange = 0 };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        var draws = new List<HistoricalDraw>
            {
                new(1, lotteryId, DateTime.UtcNow.AddDays(-3), new List<int>{ 3, 7, 11 }, new List<int>(), DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-2), new List<int>{ 3, 8, 12 }, new List<int>(), DateTime.UtcNow),
                new(3, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int>{ 3, 7, 12 }, new List<int>(), DateTime.UtcNow),
            };
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(draws);

        // Act
        var result = await _sut.Predict(lotteryId);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task Given_Valid_Config_Without_Bonuses_When_Predict_Is_Invoked_Should_Return_Value()
    {
        // Arrange
        const int lotteryId = 9001;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 3, MainNumbersRange = 30, BonusNumbersCount = 0, BonusNumbersRange = 0 };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        var draws = new List<HistoricalDraw>
            {
                new(1, lotteryId, DateTime.UtcNow.AddDays(-3), new List<int>{ 3, 7, 11 }, new List<int>(), DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-2), new List<int>{ 3, 8, 12 }, new List<int>(), DateTime.UtcNow),
                new(3, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int>{ 3, 7, 12 }, new List<int>(), DateTime.UtcNow),
            };
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(draws);

        // Act
        var result = await _sut.Predict(lotteryId);

        // Assert
        result.Value.Should().NotBeNull();
    }

    [Test]
    public async Task Given_Valid_Config_With_Bonuses_When_Predict_Is_Invoked_Should_Return_Exact_Main_Count()
    {
        // Arrange
        const int lotteryId = 9002;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 4, MainNumbersRange = 50, BonusNumbersCount = 2, BonusNumbersRange = 9 };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        var draws = new List<HistoricalDraw>
            {
                new(1, lotteryId, DateTime.UtcNow.AddDays(-2), new List<int>{ 10, 20, 30, 40 }, new List<int>{ 7 }, DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int>{ 10, 21, 31, 41 }, new List<int>{ 7, 8 }, DateTime.UtcNow),
            };
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(draws);

        // Act
        var prediction = (await _sut.Predict(lotteryId)).Value!;

        // Assert
        prediction.PredictedNumbers.Count().Should().Be(config.MainNumbersCount);
    }

    [Test]
    public async Task Given_Valid_Config_With_Bonuses_When_Predict_Is_Invoked_Should_Return_MainNumbers_In_Range()
    {
        // Arrange
        const int lotteryId = 9002;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 4, MainNumbersRange = 50, BonusNumbersCount = 2, BonusNumbersRange = 9 };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        var draws = new List<HistoricalDraw>
            {
                new(1, lotteryId, DateTime.UtcNow.AddDays(-2), new List<int>{ 10, 20, 30, 40 }, new List<int>{ 7 }, DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int>{ 10, 21, 31, 41 }, new List<int>{ 7, 8 }, DateTime.UtcNow),
            };
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(draws);

        // Act
        var prediction = (await _sut.Predict(lotteryId)).Value!;

        // Assert
        prediction.PredictedNumbers.Should().OnlyContain(n => n >= 1 && n <= config.MainNumbersRange);
    }

    [Test]
    public async Task Given_Valid_Config_With_Bonuses_When_Predict_Is_Invoked_Should_Return_Exact_Bonus_Count()
    {
        // Arrange
        const int lotteryId = 9002;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 4, MainNumbersRange = 50, BonusNumbersCount = 2, BonusNumbersRange = 9 };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        var draws = new List<HistoricalDraw>
            {
                new(1, lotteryId, DateTime.UtcNow.AddDays(-2), new List<int>{ 10, 20, 30, 40 }, new List<int>{ 7 }, DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int>{ 10, 21, 31, 41 }, new List<int>{ 7, 8 }, DateTime.UtcNow),
            };
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(draws);

        // Act
        var prediction = (await _sut.Predict(lotteryId)).Value!;

        // Assert
        prediction.BonusNumbers.Count().Should().Be(config.BonusNumbersCount);
    }

    [Test]
    public async Task Given_Valid_Config_With_Bonuses_When_Predict_Is_Invoked_Should_Return_BonusNumbers_In_Range()
    {
        // Arrange
        const int lotteryId = 9002;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 4, MainNumbersRange = 50, BonusNumbersCount = 2, BonusNumbersRange = 9 };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        var draws = new List<HistoricalDraw>
            {
                new(1, lotteryId, DateTime.UtcNow.AddDays(-2), new List<int>{ 10, 20, 30, 40 }, new List<int>{ 7 }, DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int>{ 10, 21, 31, 41 }, new List<int>{ 7, 8 }, DateTime.UtcNow),
            };
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(draws);

        // Act
        var prediction = (await _sut.Predict(lotteryId)).Value!;

        // Assert
        prediction.BonusNumbers.Should().OnlyContain(n => n >= 1 && n <= config.BonusNumbersRange);
    }

    [Test]
    public async Task Given_Valid_Config_With_Bonuses_When_Predict_Is_Invoked_Should_Return_Confidence_As_Probability()
    {
        // Arrange
        const int lotteryId = 9002;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 4, MainNumbersRange = 50, BonusNumbersCount = 2, BonusNumbersRange = 9 };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        var draws = new List<HistoricalDraw>
            {
                new(1, lotteryId, DateTime.UtcNow.AddDays(-2), new List<int>{ 10, 20, 30, 40 }, new List<int>{ 7 }, DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int>{ 10, 21, 31, 41 }, new List<int>{ 7, 8 }, DateTime.UtcNow),
            };
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(draws);

        // Act
        var prediction = (await _sut.Predict(lotteryId)).Value!;

        // Assert
        prediction.ConfidenceScore.Should().BeInRange(0.0, 1.0);
    }

    [Test]
    public async Task Given_Clear_Frequencies_When_Predict_Is_Invoked_Should_Select_Top_Frequent_Main()
    {
        // Arrange
        // Frequencies: 1(3x), 2(2x), 5(2x), 3(1x), 4(1x) -> expect [1,2,5]
        const int lotteryId = 9003;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 3, MainNumbersRange = 20, BonusNumbersCount = 0, BonusNumbersRange = 0 };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        var draws = new List<HistoricalDraw>
            {
                new(1, lotteryId, DateTime.UtcNow.AddDays(-3), new List<int>{ 1, 4, 5 }, new List<int>(), DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-2), new List<int>{ 1, 2, 5 }, new List<int>(), DateTime.UtcNow),
                new(3, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int>{ 1, 2, 3 }, new List<int>(), DateTime.UtcNow),
            };
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(draws);

        // Act
        var prediction = (await _sut.Predict(lotteryId)).Value!;

        // Assert
        prediction.PredictedNumbers.Should().Equal(1, 2, 5);
    }

    [Test]
    public async Task Given_Clear_Frequencies_When_Predict_Is_Invoked_Should_Compute_Expected_Confidence()
    {
        // Arrange
        const int lotteryId = 9003;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 3, MainNumbersRange = 20, BonusNumbersCount = 0, BonusNumbersRange = 0 };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        var draws = new List<HistoricalDraw>
            {
                new(1, lotteryId, DateTime.UtcNow.AddDays(-3), new List<int>{ 1, 4, 5 }, new List<int>(), DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-2), new List<int>{ 1, 2, 5 }, new List<int>(), DateTime.UtcNow),
                new(3, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int>{ 1, 2, 3 }, new List<int>(), DateTime.UtcNow),
            };
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(draws);

        // Act
        var prediction = (await _sut.Predict(lotteryId)).Value!;

        // Assert
        prediction.ConfidenceScore.Should().BeApproximately(7.0 / 9.0, 1e-9);
    }

    [Test]
    public async Task Given_Ties_In_Frequency_When_Predict_Is_Invoked_Should_Prefer_Lower_Numbers()
    {
        // Arrange
        // 1,2,3 all once; pick first 2 -> [1,2]
        const int lotteryId = 9004;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 2, MainNumbersRange = 10, BonusNumbersCount = 0, BonusNumbersRange = 0 };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        var draws = new List<HistoricalDraw>
            {
                new(1, lotteryId, DateTime.UtcNow.AddDays(-3), new List<int>{ 1 }, new List<int>(), DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-2), new List<int>{ 2 }, new List<int>(), DateTime.UtcNow),
                new(3, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int>{ 3 }, new List<int>(), DateTime.UtcNow),
            };
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(draws);

        // Act
        var prediction = (await _sut.Predict(lotteryId)).Value!;

        // Assert
        prediction.PredictedNumbers.Should().Equal(1, 2);
    }

    // ---------- NEGATIVE-STYLE ASSERTIONS (invariants phrased as "should not") ----------

    [Test]
    public async Task Given_Valid_Config_When_Predict_Is_Invoked_Should_Not_Exceed_MainNumbersCount()
    {
        // Arrange
        const int lotteryId = 9005;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 4, MainNumbersRange = 40, BonusNumbersCount = 0, BonusNumbersRange = 0 };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        var draws = new List<HistoricalDraw>
            {
                new(1, lotteryId, DateTime.UtcNow.AddDays(-2), new List<int>{ 4, 7, 12, 18 }, new List<int>(), DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int>{ 4, 7, 12, 18 }, new List<int>(), DateTime.UtcNow),
            };
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(draws);

        // Act
        var prediction = (await _sut.Predict(lotteryId)).Value!;

        // Assert
        prediction.PredictedNumbers.Count().Should().BeLessOrEqualTo(config.MainNumbersCount);
    }

    [Test]
    public async Task Given_Valid_Config_When_Predict_Is_Invoked_Should_Not_Return_MainNumbers_Outside_Range()
    {
        // Arrange
        const int lotteryId = 9006;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 3, MainNumbersRange = 25, BonusNumbersCount = 2, BonusNumbersRange = 10 };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        var draws = new List<HistoricalDraw>
            {
                new(1, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int>{ 2, 6, 10 }, new List<int>{ 3 }, DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-2), new List<int>{ 2, 6, 10 }, new List<int>{ 4 }, DateTime.UtcNow),
            };
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(draws);

        // Act
        var prediction = (await _sut.Predict(lotteryId)).Value!;

        // Assert
        prediction.PredictedNumbers.Should().OnlyContain(n => n >= 1 && n <= config.MainNumbersRange);
    }

    [Test]
    public async Task Given_Valid_Config_When_Predict_Is_Invoked_Should_Not_Return_BonusNumbers_Outside_Range()
    {
        // Arrange
        const int lotteryId = 9006;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 3, MainNumbersRange = 25, BonusNumbersCount = 2, BonusNumbersRange = 10 };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        var draws = new List<HistoricalDraw>
            {
                new(1, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int>{ 2, 6, 10 }, new List<int>{ 3 }, DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-2), new List<int>{ 2, 6, 10 }, new List<int>{ 4 }, DateTime.UtcNow),
            };
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(draws);

        // Act
        var prediction = (await _sut.Predict(lotteryId)).Value!;

        // Assert
        prediction.BonusNumbers.Should().OnlyContain(n => n >= 1 && n <= config.BonusNumbersRange);
    }

    [Test]
    public async Task Given_Valid_Config_When_Predict_Is_Invoked_Should_Not_Return_Confidence_Out_Of_Range()
    {
        // Arrange
        const int lotteryId = 9006;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 3, MainNumbersRange = 25, BonusNumbersCount = 2, BonusNumbersRange = 10 };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        var draws = new List<HistoricalDraw>
            {
                new(1, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int>{ 2, 6, 10 }, new List<int>{ 3 }, DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-2), new List<int>{ 2, 6, 10 }, new List<int>{ 4 }, DateTime.UtcNow),
            };
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(draws);

        // Act
        var prediction = (await _sut.Predict(lotteryId)).Value!;

        // Assert
        prediction.ConfidenceScore.Should().BeInRange(0.0, 1.0);
    }

    // ---------- FAILURE / ERROR NEGATIVE CASES ----------

    [Test]
    public async Task Given_No_Active_Configuration_When_Predict_Is_Invoked_Should_Fail()
    {
        // Arrange
        const int lotteryId = 9991;
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(Task.FromResult<LotteryConfigurationDomain?>(null));

        // Act
        var result = await _sut.Predict(lotteryId);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    [Test]
    public async Task Given_No_Active_Configuration_When_Predict_Is_Invoked_Should_Include_LotteryId_In_Errors()
    {
        // Arrange
        const int lotteryId = 9991;
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
        const int lotteryId = 9992;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 3, MainNumbersRange = 30, BonusNumbersCount = 0, BonusNumbersRange = 0 };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(new List<HistoricalDraw>());

        // Act
        var result = await _sut.Predict(lotteryId);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    [Test]
    public async Task Given_Empty_History_When_Predict_Is_Invoked_Should_Include_LotteryId_In_Errors()
    {
        // Arrange
        const int lotteryId = 9992;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 3, MainNumbersRange = 30, BonusNumbersCount = 0, BonusNumbersRange = 0 };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(new List<HistoricalDraw>());

        // Act
        var result = await _sut.Predict(lotteryId);

        // Assert
        result.Errors.Should().Contain(e => e.Contains(lotteryId.ToString()));
    }
}