using FluentAssertions;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Repositories;
using JackpotPlot.Domain.Services.PredictionStrategies;
using NSubstitute;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.ServicesTests.PredictionStrategiesTests.CyclicPatternsPredictionStrategyTests;

[TestFixture]
public class PredictTests
{
    private ILotteryConfigurationRepository _configRepo = null!;
    private ILotteryHistoryRepository _historyRepo = null!;
    private CyclicPatternsPredictionStrategy _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _configRepo = Substitute.For<ILotteryConfigurationRepository>();
        _historyRepo = Substitute.For<ILotteryHistoryRepository>();
        _sut = new CyclicPatternsPredictionStrategy(_configRepo, _historyRepo);
    }

    // ---------- POSITIVE-STYLE ASSERTIONS ----------

    [Test]
    public async Task Given_Valid_Config_With_Bonuses_When_Predict_Is_Invoked_Should_Succeed()
    {
        // Arrange
        const int lotteryId = 504;
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
                new(1, lotteryId, DateTime.UtcNow.AddDays(-5), new List<int>{ 4,5,6 }, new List<int>(), DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-3), new List<int>{ 5,6,7 }, new List<int>(), DateTime.UtcNow),
                new(3, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int>{ 6,7,8 }, new List<int>(), DateTime.UtcNow),
            };
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(draws);

        // Act
        var result = await _sut.Predict(lotteryId);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task Given_Valid_Config_With_Bonuses_When_Predict_Is_Invoked_Should_Return_At_Least_One_MainNumber()
    {
        // Arrange
        const int lotteryId = 504;
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
                new(1, lotteryId, DateTime.UtcNow.AddDays(-5), new List<int>{ 4,5,6 }, new List<int>(), DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-3), new List<int>{ 5,6,7 }, new List<int>(), DateTime.UtcNow),
                new(3, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int>{ 6,7,8 }, new List<int>(), DateTime.UtcNow),
            };
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(draws);

        // Act
        var result = await _sut.Predict(lotteryId);
        var prediction = result.Value!;

        // Assert
        prediction.PredictedNumbers.Count().Should().BeGreaterThan(0);
    }

    [Test]
    public async Task Given_Valid_Config_With_Bonuses_When_Predict_Is_Invoked_Should_Return_Exactly_Configured_BonusNumbers_Count()
    {
        // Arrange
        const int lotteryId = 504;
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
                new(1, lotteryId, DateTime.UtcNow.AddDays(-5), new List<int>{ 4,5,6 }, new List<int>(), DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-3), new List<int>{ 5,6,7 }, new List<int>(), DateTime.UtcNow),
                new(3, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int>{ 6,7,8 }, new List<int>(), DateTime.UtcNow),
            };
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(draws);

        // Act
        var result = await _sut.Predict(lotteryId);
        var prediction = result.Value!;

        // Assert
        prediction.BonusNumbers.Count().Should().Be(config.BonusNumbersCount);
    }

    [Test]
    public async Task Given_Valid_Config_With_Bonuses_When_Predict_Is_Invoked_Should_Return_ConfidenceScore_As_Probability()
    {
        // Arrange
        const int lotteryId = 504;
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
                new(1, lotteryId, DateTime.UtcNow.AddDays(-5), new List<int>{ 4,5,6 }, new List<int>(), DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-3), new List<int>{ 5,6,7 }, new List<int>(), DateTime.UtcNow),
                new(3, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int>{ 6,7,8 }, new List<int>(), DateTime.UtcNow),
            };
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(draws);

        // Act
        var result = await _sut.Predict(lotteryId);
        var prediction = result.Value!;

        // Assert
        prediction.ConfidenceScore.Should().BeGreaterOrEqualTo(0.0).And.BeLessOrEqualTo(1.0);
    }

    // ---------- NEGATIVE-STYLE ASSERTIONS (invariants phrased as "should not") ----------

    [Test]
    public async Task Given_Valid_Config_With_Bonuses_When_Predict_Is_Invoked_Should_Not_Exceed_MainNumbersCount()
    {
        // Arrange
        const int lotteryId = 504;
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
                new(1, lotteryId, DateTime.UtcNow.AddDays(-5), new List<int>{ 4,5,6 }, new List<int>(), DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-3), new List<int>{ 5,6,7 }, new List<int>(), DateTime.UtcNow),
                new(3, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int>{ 6,7,8 }, new List<int>(), DateTime.UtcNow),
            };
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(draws);

        // Act
        var result = await _sut.Predict(lotteryId);
        var prediction = result.Value!;

        // Assert
        prediction.PredictedNumbers.Count().Should().BeLessOrEqualTo(config.MainNumbersCount);
    }

    [Test]
    public async Task Given_Valid_Config_With_Bonuses_When_Predict_Is_Invoked_Should_Not_Return_MainNumbers_Outside_Configured_Range()
    {
        // Arrange
        const int lotteryId = 504;
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
                new(1, lotteryId, DateTime.UtcNow.AddDays(-5), new List<int>{ 4,5,6 }, new List<int>(), DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-3), new List<int>{ 5,6,7 }, new List<int>(), DateTime.UtcNow),
                new(3, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int>{ 6,7,8 }, new List<int>(), DateTime.UtcNow),
            };
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(draws);

        // Act
        var result = await _sut.Predict(lotteryId);
        var prediction = result.Value!;

        // Assert
        prediction.PredictedNumbers.Should().OnlyContain(n => n >= 1 && n <= config.MainNumbersRange);
    }

    [Test]
    public async Task Given_Valid_Config_With_Bonuses_When_Predict_Is_Invoked_Should_Not_Return_More_Than_Configured_BonusNumbers()
    {
        // Arrange
        const int lotteryId = 504;
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
                new(1, lotteryId, DateTime.UtcNow.AddDays(-5), new List<int>{ 4,5,6 }, new List<int>(), DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-3), new List<int>{ 5,6,7 }, new List<int>(), DateTime.UtcNow),
                new(3, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int>{ 6,7,8 }, new List<int>(), DateTime.UtcNow),
            };
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(draws);

        // Act
        var result = await _sut.Predict(lotteryId);
        var prediction = result.Value!;

        // Assert
        prediction.BonusNumbers.Count().Should().BeLessOrEqualTo(config.BonusNumbersCount);
    }

    [Test]
    public async Task Given_Valid_Config_With_Bonuses_When_Predict_Is_Invoked_Should_Not_Return_BonusNumbers_Outside_Configured_Range()
    {
        // Arrange
        const int lotteryId = 504;
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
                new(1, lotteryId, DateTime.UtcNow.AddDays(-5), new List<int>{ 4,5,6 }, new List<int>(), DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-3), new List<int>{ 5,6,7 }, new List<int>(), DateTime.UtcNow),
                new(3, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int>{ 6,7,8 }, new List<int>(), DateTime.UtcNow),
            };
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(draws);

        // Act
        var result = await _sut.Predict(lotteryId);
        var prediction = result.Value!;

        // Assert
        prediction.BonusNumbers.Should().OnlyContain(n => n >= 1 && n <= config.BonusNumbersRange);
    }

    [Test]
    public async Task Given_Valid_Config_With_Bonuses_When_Predict_Is_Invoked_Should_Not_Return_ConfidenceScore_Out_Of_Range()
    {
        // Arrange
        const int lotteryId = 504;
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
                new(1, lotteryId, DateTime.UtcNow.AddDays(-5), new List<int>{ 4,5,6 }, new List<int>(), DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-3), new List<int>{ 5,6,7 }, new List<int>(), DateTime.UtcNow),
                new(3, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int>{ 6,7,8 }, new List<int>(), DateTime.UtcNow),
            };
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(draws);

        // Act
        var result = await _sut.Predict(lotteryId);
        var prediction = result.Value!;

        // Assert
        prediction.ConfidenceScore.Should().BeGreaterOrEqualTo(0.0).And.BeLessOrEqualTo(1.0);
    }

    // ---------- FAILURE / ERROR NEGATIVE CASES ----------

    [Test]
    public async Task Given_No_Active_Configuration_When_Predict_Is_Invoked_Should_Fail_And_Errors_Should_Contain_LotteryId()
    {
        // Arrange
        const int lotteryId = 987;
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(Task.FromResult<LotteryConfigurationDomain?>(null));

        // Act
        var result = await _sut.Predict(lotteryId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeNullOrEmpty();
        result.Errors.Should().Contain(e => e.Contains(lotteryId.ToString()));
    }

    [Test]
    public async Task Given_Empty_History_When_Predict_Is_Invoked_Should_Fail_And_Errors_Should_Contain_LotteryId()
    {
        // Arrange
        const int lotteryId = 988;
        var config = new LotteryConfigurationDomain
        {
            LotteryId = lotteryId,
            MainNumbersCount = 3,
            MainNumbersRange = 20,
            BonusNumbersCount = 2,
            BonusNumbersRange = 9
        };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(Task.FromResult<ICollection<HistoricalDraw>>(new List<HistoricalDraw>()));

        // Act
        var result = await _sut.Predict(lotteryId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeNullOrEmpty();
        result.Errors.Should().Contain(e => e.Contains(lotteryId.ToString()));
    }
}

