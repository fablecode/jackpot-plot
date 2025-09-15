using FluentAssertions;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Repositories;
using JackpotPlot.Domain.Services.PredictionStrategies;
using NSubstitute;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.ServicesTests.PredictionStrategiesTests.LastAppearancePredictionStrategyTests;

[TestFixture]
public class PredictTests
{
    private ILotteryConfigurationRepository _configRepo = null!;
    private ILotteryHistoryRepository _historyRepo = null!;
    private LastAppearancePredictionStrategy _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _configRepo = Substitute.For<ILotteryConfigurationRepository>();
        _historyRepo = Substitute.For<ILotteryHistoryRepository>();
        _sut = new LastAppearancePredictionStrategy(_configRepo, _historyRepo);
    }

    // ---------- POSITIVE: base success ----------

    [Test]
    public async Task Given_Valid_Config_And_History_When_Predict_Is_Invoked_Should_Succeed()
    {
        // Arrange
        const int lotteryId = 6101;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 3, MainNumbersRange = 10, BonusNumbersCount = 0, BonusNumbersRange = 0 };
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
        const int lotteryId = 6101;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 3, MainNumbersRange = 10, BonusNumbersCount = 0, BonusNumbersRange = 0 };
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

    // ---------- POSITIVE: counts & ranges ----------

    [Test]
    public async Task Given_Valid_Config_When_Predict_Is_Invoked_Should_Return_Exactly_Configured_Main_Count()
    {
        // Arrange
        const int lotteryId = 6102;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 2, MainNumbersRange = 5, BonusNumbersCount = 0, BonusNumbersRange = 0 };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(new List<HistoricalDraw>
            {
                new(1, lotteryId, DateTime.UtcNow.AddDays(-2), new List<int>{ 1, 2 }, new List<int>(), DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int>{ 2, 3 }, new List<int>(), DateTime.UtcNow),
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
        const int lotteryId = 6102;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 2, MainNumbersRange = 5, BonusNumbersCount = 0, BonusNumbersRange = 0 };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(new List<HistoricalDraw>
            {
                new(1, lotteryId, DateTime.UtcNow.AddDays(-2), new List<int>{ 1, 2 }, new List<int>(), DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int>{ 2, 3 }, new List<int>(), DateTime.UtcNow),
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
        const int lotteryId = 6103;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 3, MainNumbersRange = 10, BonusNumbersCount = 0, BonusNumbersRange = 0 };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(new List<HistoricalDraw>
            {
                new(1, lotteryId, DateTime.UtcNow.AddDays(-3), new List<int>{ 1, 2, 3 }, new List<int>(), DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int>{ 2, 3, 4 }, new List<int>(), DateTime.UtcNow),
            });

        // Act
        var prediction = (await _sut.Predict(lotteryId)).Value!;

        // Assert
        prediction.PredictedNumbers.Should().OnlyHaveUniqueItems();
    }

    // ---------- POSITIVE: deterministic selection & confidence ----------

    [Test]
    public async Task Given_Partial_Coverage_When_Predict_Is_Invoked_Should_Select_Most_Overdue_Numbers()
    {
        // Arrange
        // Range=5, Count=2. History:
        // d1 (oldest): [1,2]
        // d2:          [2,3]
        // d3 (latest): [3,4]
        // Last appearances: 5 -> MaxValue (never), 1 -> 3, 2 -> 2, 3 -> 1, 4 -> 1
        // So predicted = [5,1]
        const int lotteryId = 6110;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 2, MainNumbersRange = 5, BonusNumbersCount = 0, BonusNumbersRange = 0 };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);

        var draws = new List<HistoricalDraw>
            {
                new(1, lotteryId, DateTime.UtcNow.AddDays(-5), new List<int>{ 1, 2 }, new List<int>(), DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-3), new List<int>{ 2, 3 }, new List<int>(), DateTime.UtcNow),
                new(3, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int>{ 3, 4 }, new List<int>(), DateTime.UtcNow),
            };
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(draws);

        // Act
        var prediction = (await _sut.Predict(lotteryId)).Value!;

        // Assert
        prediction.PredictedNumbers.Should().Equal(5, 1);
    }

    [Test]
    public async Task Given_Partial_Coverage_When_Predict_Is_Invoked_Should_Compute_Expected_Confidence()
    {
        // Arrange
        // From the previous scenario: predicted [5,1]
        // Matches across draws: only "1" once => 1 match; denominator = 3 draws * 2 predicted = 6 -> 1/6
        const int lotteryId = 6110;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 2, MainNumbersRange = 5, BonusNumbersCount = 0, BonusNumbersRange = 0 };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(new List<HistoricalDraw>
            {
                new(1, lotteryId, DateTime.UtcNow.AddDays(-5), new List<int>{ 1, 2 }, new List<int>(), DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-3), new List<int>{ 2, 3 }, new List<int>(), DateTime.UtcNow),
                new(3, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int>{ 3, 4 }, new List<int>(), DateTime.UtcNow),
            });

        // Act
        var prediction = (await _sut.Predict(lotteryId)).Value!;

        // Assert
        prediction.ConfidenceScore.Should().BeApproximately(1.0 / 6.0, 1e-9);
    }

    [Test]
    public async Task Given_Full_Coverage_When_Predict_Is_Invoked_Should_Select_Oldest_Last_Appearances()
    {
        // Arrange
        // Range=4, Count=2. History (oldest -> latest):
        // d1: [1,4], d2: [2,4], d3: [3,4]
        // Last appearances: 1->3, 2->2, 3->1, 4->1 -> predicted [1,2]
        const int lotteryId = 6111;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 2, MainNumbersRange = 4, BonusNumbersCount = 0, BonusNumbersRange = 0 };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);

        var draws = new List<HistoricalDraw>
            {
                new(1, lotteryId, DateTime.UtcNow.AddDays(-5), new List<int>{ 1, 4 }, new List<int>(), DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-3), new List<int>{ 2, 4 }, new List<int>(), DateTime.UtcNow),
                new(3, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int>{ 3, 4 }, new List<int>(), DateTime.UtcNow),
            };
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(draws);

        // Act
        var prediction = (await _sut.Predict(lotteryId)).Value!;

        // Assert
        prediction.PredictedNumbers.Should().Equal(1, 2);
    }

    [Test]
    public async Task Given_Full_Coverage_When_Predict_Is_Invoked_Should_Compute_Expected_Confidence()
    {
        // Arrange
        // From the previous scenario: predicted [1,2]
        // Matches: 1 once + 2 once = 2; denominator = 3*2 = 6 -> 1/3
        const int lotteryId = 6111;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 2, MainNumbersRange = 4, BonusNumbersCount = 0, BonusNumbersRange = 0 };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(new List<HistoricalDraw>
            {
                new(1, lotteryId, DateTime.UtcNow.AddDays(-5), new List<int>{ 1, 4 }, new List<int>(), DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-3), new List<int>{ 2, 4 }, new List<int>(), DateTime.UtcNow),
                new(3, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int>{ 3, 4 }, new List<int>(), DateTime.UtcNow),
            });

        // Act
        var prediction = (await _sut.Predict(lotteryId)).Value!;

        // Assert
        prediction.ConfidenceScore.Should().BeApproximately(1.0 / 3.0, 1e-9);
    }

    // ---------- POSITIVE: bonuses ----------

    [Test]
    public async Task Given_Bonus_Config_When_Predict_Is_Invoked_Should_Return_Exact_Bonus_Count()
    {
        // Arrange
        const int lotteryId = 6120;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 3, MainNumbersRange = 10, BonusNumbersCount = 2, BonusNumbersRange = 9 };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(new List<HistoricalDraw>
            {
                new(1, lotteryId, DateTime.UtcNow.AddDays(-2), new List<int>{ 1, 2, 3 }, new List<int>(), DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int>{ 2, 3, 4 }, new List<int>(), DateTime.UtcNow),
            });

        // Act
        var prediction = (await _sut.Predict(lotteryId)).Value!;

        // Assert
        prediction.BonusNumbers.Count().Should().Be(config.BonusNumbersCount);
    }

    [Test]
    public async Task Given_Bonus_Config_When_Predict_Is_Invoked_Should_Return_BonusNumbers_In_Range()
    {
        // Arrange
        const int lotteryId = 6120;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 3, MainNumbersRange = 10, BonusNumbersCount = 2, BonusNumbersRange = 9 };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(new List<HistoricalDraw>
            {
                new(1, lotteryId, DateTime.UtcNow.AddDays(-2), new List<int>{ 1, 2, 3 }, new List<int>(), DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int>{ 2, 3, 4 }, new List<int>(), DateTime.UtcNow),
            });

        // Act
        var prediction = (await _sut.Predict(lotteryId)).Value!;

        // Assert
        prediction.BonusNumbers.Should().OnlyContain(n => n >= 1 && n <= config.BonusNumbersRange);
    }

    [Test]
    public async Task Given_Bonus_Config_When_Predict_Is_Invoked_Should_Return_Unique_BonusNumbers()
    {
        // Arrange
        const int lotteryId = 6120;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 3, MainNumbersRange = 10, BonusNumbersCount = 2, BonusNumbersRange = 9 };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(new List<HistoricalDraw>
            {
                new(1, lotteryId, DateTime.UtcNow.AddDays(-2), new List<int>{ 1, 2, 3 }, new List<int>(), DateTime.UtcNow),
                new(2, lotteryId, DateTime.UtcNow.AddDays(-1), new List<int>{ 2, 3, 4 }, new List<int>(), DateTime.UtcNow),
            });

        // Act
        var prediction = (await _sut.Predict(lotteryId)).Value!;

        // Assert
        prediction.BonusNumbers.Should().OnlyHaveUniqueItems();
    }

    // ---------- FAILURE / ERROR NEGATIVE CASES ----------

    [Test]
    public async Task Given_No_Active_Configuration_When_Predict_Is_Invoked_Should_Fail()
    {
        // Arrange
        const int lotteryId = 6990;
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
        const int lotteryId = 6991;
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
        const int lotteryId = 6992;
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
        const int lotteryId = 6993;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 3, MainNumbersRange = 10, BonusNumbersCount = 0, BonusNumbersRange = 0 };
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
        const int lotteryId = 6994;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 3, MainNumbersRange = 10, BonusNumbersCount = 0, BonusNumbersRange = 0 };
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
        const int lotteryId = 6995;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 3, MainNumbersRange = 10, BonusNumbersCount = 0, BonusNumbersRange = 0 };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(new List<HistoricalDraw>());

        // Act
        var result = await _sut.Predict(lotteryId);

        // Assert
        result.Errors.Should().Contain(e => e.Contains(lotteryId.ToString()));
    }
}