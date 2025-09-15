using FluentAssertions;
using JackpotPlot.Domain.Repositories;
using JackpotPlot.Domain.Services.PredictionStrategies;
using NSubstitute;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.ServicesTests.PredictionStrategiesTests.InvertedFrequencyPredictionStrategyTests;

[TestFixture]
public class HandlesTests
{
    private ILotteryConfigurationRepository _configRepo = null!;
    private ILotteryHistoryRepository _historyRepo = null!;
    private InvertedFrequencyPredictionStrategy _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _configRepo = Substitute.For<ILotteryConfigurationRepository>();
        _historyRepo = Substitute.For<ILotteryHistoryRepository>();
        _sut = new InvertedFrequencyPredictionStrategy(_configRepo, _historyRepo);
    }

    // ---------- Negative cases ----------
    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    [TestCase("SomeOtherStrategy")]
    public void Given_An_Invalid_Strategy_Should_Return_False(string strategy)
    {
        // Arrange
        // Act
        var result = _sut.Handles(strategy);

        // Assert
        result.Should().BeFalse();
    }

    // ---------- Positive cases (case-insensitive match) ----------
    [TestCase("inverted-frequency")]
    [TestCase("INVERTED-FREQUENCY")]
    [TestCase("InVeRtEd-FrEqUeNcY")]
    public void Given_A_Valid_Strategy_Should_Return_True(string strategy)
    {
        // Arrange
        // Act
        var result = _sut.Handles(strategy);

        // Assert
        result.Should().BeTrue();
    }
}