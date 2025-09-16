using System.Collections.Immutable;
using FluentAssertions;
using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Interfaces;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Repositories;
using JackpotPlot.Domain.Services.PredictionStrategies;
using JackpotPlot.Domain.ValueObjects;
using NSubstitute;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.ServicesTests.PredictionStrategiesTests.MixedPredictionStrategyTests;

[TestFixture]
public class PredictTests
{
    private ILotteryConfigurationRepository _configRepo = null!;
    private ILotteryHistoryRepository _historyRepo = null!;

    [SetUp]
    public void SetUp()
    {
        _configRepo = Substitute.For<ILotteryConfigurationRepository>();
        _historyRepo = Substitute.For<ILotteryHistoryRepository>();
    }

    // ---------- helpers ----------

    private static IPredictionStrategy MakeStrategy(
        string name,
        IEnumerable<int> main,
        IEnumerable<int> bonus,
        double confidence)
    {
        var strat = Substitute.For<IPredictionStrategy>();
        strat.Predict(Arg.Any<int>()).Returns(ci =>
        {
            var id = ci.Arg<int>();
            var pr = new PredictionResult(
                id,
                main.ToImmutableArray(),
                bonus.ToImmutableArray(),
                confidence,
                name);
            return Task.FromResult(Result<PredictionResult>.Success(pr));
        });
        return strat;
    }

    private static List<HistoricalDraw> SomeHistory(int lotteryId) => new()
        {
            new(1, lotteryId, DateTime.UtcNow.AddDays(-2), new List<int>{ 1,2,3 }, new List<int>(), DateTime.UtcNow)
        };

    // ---------- POSITIVE: base success ----------

    [Test]
    public async Task Given_Valid_Config_And_History_When_Predict_Is_Invoked_Should_Succeed()
    {
        // Arrange
        const int lotteryId = 7001;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 3, MainNumbersRange = 50, BonusNumbersCount = 0, BonusNumbersRange = 0 };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(SomeHistory(lotteryId));

        var a = MakeStrategy("A", new[] { 1, 2, 3 }, Array.Empty<int>(), 0.6);
        var b = MakeStrategy("B", new[] { 3, 4, 5 }, Array.Empty<int>(), 0.2);
        var strategies = new List<IPredictionStrategy> { a, b };
        var weights = new Dictionary<string, double> { ["A"] = 2.0, ["B"] = 1.0 };

        var sut = new MixedPredictionStrategy(strategies, weights, _configRepo, _historyRepo);

        // Act
        var result = await sut.Predict(lotteryId);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task Given_Valid_Config_And_History_When_Predict_Is_Invoked_Should_Return_Value()
    {
        // Arrange
        const int lotteryId = 7001;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 3, MainNumbersRange = 50, BonusNumbersCount = 0, BonusNumbersRange = 0 };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(SomeHistory(lotteryId));

        var a = MakeStrategy("A", new[] { 1, 2, 3 }, Array.Empty<int>(), 0.6);
        var b = MakeStrategy("B", new[] { 3, 4, 5 }, Array.Empty<int>(), 0.2);
        var sut = new MixedPredictionStrategy(new() { a, b }, new() { ["A"] = 2.0, ["B"] = 1.0 }, _configRepo, _historyRepo);

        // Act
        var result = await sut.Predict(lotteryId);

        // Assert
        result.Value.Should().NotBeNull();
    }

    // ---------- POSITIVE: counts, ranges, composition ----------

    [Test]
    public async Task Given_Weighted_Strategies_When_Predict_Is_Invoked_Should_Return_Exactly_Configured_Main_Count()
    {
        // Arrange
        const int lotteryId = 7002;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 3, MainNumbersRange = 50, BonusNumbersCount = 0, BonusNumbersRange = 0 };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(SomeHistory(lotteryId));

        var a = MakeStrategy("A", new[] { 1, 2, 3 }, Array.Empty<int>(), 0.6);
        var b = MakeStrategy("B", new[] { 3, 4, 5 }, Array.Empty<int>(), 0.2);
        var sut = new MixedPredictionStrategy(new() { a, b }, new() { ["A"] = 2.0, ["B"] = 1.0 }, _configRepo, _historyRepo);

        // Act
        var prediction = (await sut.Predict(lotteryId)).Value;

        // Assert
        prediction.PredictedNumbers.Count().Should().Be(config.MainNumbersCount);
    }

    [Test]
    public async Task Given_Weighted_Strategies_When_Predict_Is_Invoked_Should_Combine_By_Weights()
    {
        // Arrange
        // Totals: 1->2, 2->2, 3->3 (A=2, B=1), 4->1, 5->1 => top 3 = [3,1,2]
        const int lotteryId = 7003;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 3, MainNumbersRange = 50, BonusNumbersCount = 0, BonusNumbersRange = 0 };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(SomeHistory(lotteryId));

        var a = MakeStrategy("A", new[] { 1, 2, 3 }, Array.Empty<int>(), 0.6);
        var b = MakeStrategy("B", new[] { 3, 4, 5 }, Array.Empty<int>(), 0.2);
        var sut = new MixedPredictionStrategy(new() { a, b }, new() { ["A"] = 2.0, ["B"] = 1.0 }, _configRepo, _historyRepo);

        // Act
        var prediction = (await sut.Predict(lotteryId)).Value;

        // Assert
        prediction.PredictedNumbers.Should().Equal(3, 1, 2);
    }

    [Test]
    public async Task Given_Valid_Config_When_Predict_Is_Invoked_Should_Return_MainNumbers_In_Range()
    {
        // Arrange
        const int lotteryId = 7004;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 3, MainNumbersRange = 50, BonusNumbersCount = 0, BonusNumbersRange = 0 };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(SomeHistory(lotteryId));

        var a = MakeStrategy("A", new[] { 1, 2, 3 }, Array.Empty<int>(), 0.6);
        var b = MakeStrategy("B", new[] { 3, 4, 5 }, Array.Empty<int>(), 0.2);
        var sut = new MixedPredictionStrategy(new() { a, b }, new() { ["A"] = 2.0, ["B"] = 1.0 }, _configRepo, _historyRepo);

        // Act
        var prediction = (await sut.Predict(lotteryId)).Value;

        // Assert
        prediction.PredictedNumbers.Should().OnlyContain(n => n >= 1 && n <= config.MainNumbersRange);
    }

    [Test]
    public async Task Given_Strategies_With_Bonuses_When_Predict_Is_Invoked_Should_Combine_Bonuses_By_Frequency()
    {
        // Arrange
        // A: [7,8], B: [8,9], C: [8,9] -> top2 = [8,9]
        const int lotteryId = 7005;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 3, MainNumbersRange = 50, BonusNumbersCount = 2, BonusNumbersRange = 10 };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(SomeHistory(lotteryId));

        var a = MakeStrategy("A", new[] { 1, 2, 3 }, new[] { 7, 8 }, 0.6);
        var b = MakeStrategy("B", new[] { 3, 4, 5 }, new[] { 8, 9 }, 0.2);
        var c = MakeStrategy("C", new[] { 10, 11, 12 }, new[] { 8, 9 }, 0.4);

        var sut = new MixedPredictionStrategy(
            new() { a, b, c },
            new() { ["A"] = 2.0, ["B"] = 1.0, ["C"] = 1.0 },
            _configRepo, _historyRepo);

        // Act
        var prediction = (await sut.Predict(lotteryId)).Value;

        // Assert
        prediction.BonusNumbers.Should().Equal(8, 9);
    }

    [Test]
    public async Task Given_Weighted_Strategies_When_Predict_Is_Invoked_Should_Return_Weighted_Average_Confidence()
    {
        // Arrange
        // Conf = (0.6*2 + 0.2*1 + 0.4*1) / (2+1+1) = 1.8/4 = 0.45
        const int lotteryId = 7006;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 3, MainNumbersRange = 50, BonusNumbersCount = 0, BonusNumbersRange = 0 };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(SomeHistory(lotteryId));

        var a = MakeStrategy("A", new[] { 1, 2, 3 }, Array.Empty<int>(), 0.6);
        var b = MakeStrategy("B", new[] { 3, 4, 5 }, Array.Empty<int>(), 0.2);
        var c = MakeStrategy("C", new[] { 6, 7, 8 }, Array.Empty<int>(), 0.4);

        var sut = new MixedPredictionStrategy(
            new() { a, b, c },
            new() { ["A"] = 2.0, ["B"] = 1.0, ["C"] = 1.0 },
            _configRepo, _historyRepo);

        // Act
        var prediction = (await sut.Predict(lotteryId)).Value;

        // Assert
        prediction.ConfidenceScore.Should().BeApproximately(0.45, 1e-12);
    }

    [Test]
    public async Task Given_Valid_Config_When_Predict_Is_Invoked_Should_Set_Strategy_To_Mixed()
    {
        // Arrange
        const int lotteryId = 7007;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 3, MainNumbersRange = 50, BonusNumbersCount = 0, BonusNumbersRange = 0 };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(SomeHistory(lotteryId));

        var a = MakeStrategy("A", new[] { 1, 2, 3 }, Array.Empty<int>(), 0.6);
        var b = MakeStrategy("B", new[] { 3, 4, 5 }, Array.Empty<int>(), 0.2);
        var sut = new MixedPredictionStrategy(new() { a, b }, new() { ["A"] = 2.0, ["B"] = 1.0 }, _configRepo, _historyRepo);

        // Act
        var prediction = (await sut.Predict(lotteryId)).Value;

        // Assert
        prediction.Strategy.Should().Be(PredictionStrategyType.Mixed);
    }

    // ---------- FAILURE / ERROR CASES ----------

    [Test]
    public async Task Given_No_Active_Configuration_When_Predict_Is_Invoked_Should_Fail()
    {
        // Arrange
        const int lotteryId = 7090;
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(Task.FromResult<LotteryConfigurationDomain?>(null));
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(new List<HistoricalDraw>()); // not reached

        var sut = new MixedPredictionStrategy(new(), new(), _configRepo, _historyRepo);

        // Act
        var result = await sut.Predict(lotteryId);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    [Test]
    public async Task Given_No_Active_Configuration_When_Predict_Is_Invoked_Errors_Should_Contain_LotteryId()
    {
        // Arrange
        const int lotteryId = 7091;
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(Task.FromResult<LotteryConfigurationDomain?>(null));
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(new List<HistoricalDraw>());

        var sut = new MixedPredictionStrategy(new(), new(), _configRepo, _historyRepo);

        // Act
        var result = await sut.Predict(lotteryId);

        // Assert
        result.Errors.Should().Contain(e => e.Contains(lotteryId.ToString()));
    }

    [Test]
    public async Task Given_Empty_History_When_Predict_Is_Invoked_Should_Fail()
    {
        // Arrange
        const int lotteryId = 7092;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 3, MainNumbersRange = 50, BonusNumbersCount = 0, BonusNumbersRange = 0 };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(new List<HistoricalDraw>());

        var sut = new MixedPredictionStrategy(new(), new(), _configRepo, _historyRepo);

        // Act
        var result = await sut.Predict(lotteryId);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    [Test]
    public async Task Given_Empty_History_When_Predict_Is_Invoked_Errors_Should_Contain_LotteryId()
    {
        // Arrange
        const int lotteryId = 7093;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId, MainNumbersCount = 3, MainNumbersRange = 50, BonusNumbersCount = 0, BonusNumbersRange = 0 };
        _configRepo.GetActiveConfigurationAsync(lotteryId).Returns(config);
        _historyRepo.GetHistoricalDraws(lotteryId).Returns(new List<HistoricalDraw>());

        var sut = new MixedPredictionStrategy(new(), new(), _configRepo, _historyRepo);

        // Act
        var result = await sut.Predict(lotteryId);

        // Assert
        result.Errors.Should().Contain(e => e.Contains(lotteryId.ToString()));
    }
}