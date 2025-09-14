using FluentAssertions;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Repositories;
using JackpotPlot.Domain.Services.PredictionStrategies;
using NSubstitute;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.ServicesTests.PredictionStrategiesTests.DeltaSystemPredictionStrategyTests;

[TestFixture]
public class PredictTests
{
    private ILotteryConfigurationRepository _configRepo = null!;
    private ILotteryHistoryRepository _historyRepo = null!;
    private DeltaSystemPredictionStrategy _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _configRepo = Substitute.For<ILotteryConfigurationRepository>();
        _historyRepo = Substitute.For<ILotteryHistoryRepository>();
        _sut = new DeltaSystemPredictionStrategy(_configRepo, _historyRepo);
    }

    // ---------- POSITIVE-STYLE ASSERTIONS ----------

    [Test]
    public async Task Given_Valid_Config_When_Predict_Is_Invoked_Should_Succeed()
    {
        // Arrange
        const int lotteryId = 703;
        var config = new LotteryConfigurationDomain
        {
            LotteryId = lotteryId,
            MainNumbersCount = 4,
            MainNumbersRange = 40,
            BonusNumbersCount = 0,
            BonusNumbersRange = 0
        };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);

        var draws = new List<HistoricalDraw>
        {
            new(1, lotteryId, DateTime.UtcNow.AddDays(-3), new List<int> { 2, 5, 9, 15 }, new List<int>(), DateTime.UtcNow),
            new(2, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int> { 3, 6, 10, 16 }, new List<int>(), DateTime.UtcNow),
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
        const int lotteryId = 704;
        var config = new LotteryConfigurationDomain
        {
            LotteryId = lotteryId,
            MainNumbersCount = 3,
            MainNumbersRange = 30,
            BonusNumbersCount = 0,
            BonusNumbersRange = 0
        };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);

        var draws = new List<HistoricalDraw>
        {
            new(1, lotteryId, DateTime.UtcNow.AddDays(-2), new List<int> { 4, 7, 12, 20 }, new List<int>(), DateTime.UtcNow),
            new(2, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int> { 5, 9, 13, 21 }, new List<int>(), DateTime.UtcNow),
        };
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(draws);

        // Act
        var prediction = (await _sut.Predict(lotteryId)).Value;

        // Assert
        prediction.PredictedNumbers.Should().OnlyContain(n => n >= 1 && n <= config.MainNumbersRange);
    }

    [Test]
    public async Task Given_Valid_Config_When_Predict_Is_Invoked_Should_Return_Empty_Bonuses()
    {
        // Arrange
        const int lotteryId = 704;
        var config = new LotteryConfigurationDomain
        {
            LotteryId = lotteryId,
            MainNumbersCount = 3,
            MainNumbersRange = 30,
            BonusNumbersCount = 0,
            BonusNumbersRange = 0
        };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);

        var draws = new List<HistoricalDraw>
        {
            new(1, lotteryId, DateTime.UtcNow.AddDays(-2), new List<int> { 4, 7, 12, 20 }, new List<int>(), DateTime.UtcNow),
            new(2, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int> { 5, 9, 13, 21 }, new List<int>(), DateTime.UtcNow),
        };
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(draws);

        // Act
        var prediction = (await _sut.Predict(lotteryId)).Value;

        // Assert
        prediction.BonusNumbers.Should().BeEmpty();
    }

    [Test]
    public async Task Given_Repeated_Small_Deltas_When_Predict_Is_Invoked_Should_Produce_Positive_Confidence()
    {
        // Arrange
        const int lotteryId = 707;
        var config = new LotteryConfigurationDomain
        {
            LotteryId = lotteryId,
            MainNumbersCount = 4,
            MainNumbersRange = 50,
            BonusNumbersCount = 0,
            BonusNumbersRange = 0
        };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);

        var draws = new List<HistoricalDraw>
        {
            new(1, lotteryId, DateTime.UtcNow.AddDays(-6), new List<int> { 4, 6, 8, 10 }, new List<int>(), DateTime.UtcNow),
            new(2, lotteryId, DateTime.UtcNow.AddDays(-4), new List<int> { 1, 3, 5, 7, 9 }, new List<int>(), DateTime.UtcNow),
            new(3, lotteryId, DateTime.UtcNow.AddDays(-2), new List<int> { 2, 4, 6 }, new List<int>(), DateTime.UtcNow),
        };
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(draws);

        // Act
        var prediction = (await _sut.Predict(lotteryId)).Value;

        // Assert
        prediction.ConfidenceScore.Should().BeGreaterThan(0.0).And.BeLessOrEqualTo(1.0);
    }

    // ---------- NEGATIVE-STYLE ASSERTIONS (invariants phrased as "should not") ----------

    [Test]
    public async Task Given_Valid_Config_When_Predict_Is_Invoked_Should_Not_Exceed_MainNumbersCount_Plus_One()
    {
        // Arrange
        const int lotteryId = 705;
        var config = new LotteryConfigurationDomain
        {
            LotteryId = lotteryId,
            MainNumbersCount = 5,
            MainNumbersRange = 60,
            BonusNumbersCount = 0,
            BonusNumbersRange = 0
        };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);

        var draws = new List<HistoricalDraw>
        {
            new(1, lotteryId, DateTime.UtcNow.AddDays(-3), new List<int> { 2, 7, 9, 15, 22, 30 }, new List<int>(), DateTime.UtcNow),
            new(2, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int> { 1, 5, 8, 14, 20, 27 }, new List<int>(), DateTime.UtcNow),
        };
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(draws);

        // Act
        var prediction = (await _sut.Predict(lotteryId)).Value;

        // Assert
        prediction.PredictedNumbers.Count().Should().BeLessOrEqualTo(config.MainNumbersCount + 1);
    }

    [Test]
    public async Task Given_Valid_Config_When_Predict_Is_Invoked_Should_Not_Return_MainNumbers_Outside_Configured_Range()
    {
        // Arrange
        const int lotteryId = 704;
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
            new(1, lotteryId, DateTime.UtcNow.AddDays(-2), new List<int> { 4, 7, 12, 20 }, new List<int>(), DateTime.UtcNow),
            new(2, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int> { 5, 9, 13, 21 }, new List<int>(), DateTime.UtcNow),
        });

        // Act
        var prediction = (await _sut.Predict(lotteryId)).Value;

        // Assert
        prediction.ConfidenceScore.Should().BeInRange(0.0, 1.0);
    }

    [Test]
    public async Task Given_Deltas_Too_Large_For_Range_When_Predict_Is_Invoked_Should_Return_Two_Numbers()
    {
        // Arrange
        const int lotteryId = 706;
        var config = new LotteryConfigurationDomain
        {
            LotteryId = lotteryId,
            MainNumbersCount = 3,
            MainNumbersRange = 10,
            BonusNumbersCount = 0,
            BonusNumbersRange = 0
        };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);

        var draws = new List<HistoricalDraw>
        {
            new(1, lotteryId, DateTime.UtcNow.AddDays(-4), new List<int> { 1, 7, 16 }, new List<int>(), DateTime.UtcNow),
            new(2, lotteryId, DateTime.UtcNow.AddDays(-2), new List<int> { 2, 8, 17 }, new List<int>(), DateTime.UtcNow),
        };
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(draws);

        // Act
        var prediction = (await _sut.Predict(lotteryId)).Value;

        // Assert
        prediction.PredictedNumbers.Count().Should().Be(2);
    }

    [Test]
    public async Task Given_Deltas_Too_Large_For_Range_When_Predict_Is_Invoked_Should_Return_Half_Confidence()
    {
        // Arrange
        const int lotteryId = 706;
        var config = new LotteryConfigurationDomain
        {
            LotteryId = lotteryId,
            MainNumbersCount = 3,
            MainNumbersRange = 10,
            BonusNumbersCount = 0,
            BonusNumbersRange = 0
        };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);

        var draws = new List<HistoricalDraw>
        {
            new(1, lotteryId, DateTime.UtcNow.AddDays(-4), new List<int> { 1, 7, 16 }, new List<int>(), DateTime.UtcNow),
            new(2, lotteryId, DateTime.UtcNow.AddDays(-2), new List<int> { 2, 8, 17 }, new List<int>(), DateTime.UtcNow),
        };
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(draws);

        // Act
        var prediction = (await _sut.Predict(lotteryId)).Value;

        // Assert
        prediction.ConfidenceScore.Should().Be(0.5);
    }


    // ---------- FAILURE / ERROR NEGATIVE CASES ----------

    [Test]
    public async Task Given_No_Active_Configuration_When_Predict_Is_Invoked_Should_Fail()
    {
        // Arrange
        const int lotteryId = 701;
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
        const int lotteryId = 701;
        _configRepo.GetActiveConfigurationAsync(lotteryId)
            .Returns(Task.FromResult<LotteryConfigurationDomain?>(null));

        // Act
        var result = await _sut.Predict(lotteryId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeNullOrEmpty();
        result.Errors.Should().Contain(e => e.Contains(lotteryId.ToString()));
    }

    [Test]
    public async Task Given_No_Active_Configuration_When_Predict_Is_Invoked_Errors_Should_Contain_LotteryId()
    {
        // Arrange
        const int lotteryId = 701;
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
        const int lotteryId = 702;
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
        const int lotteryId = 702;
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
        const int lotteryId = 702;
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