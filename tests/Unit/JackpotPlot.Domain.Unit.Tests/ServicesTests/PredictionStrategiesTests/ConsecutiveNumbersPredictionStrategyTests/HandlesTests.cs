using FluentAssertions;
using JackpotPlot.Domain.Repositories;
using JackpotPlot.Domain.Services.PredictionStrategies;
using NSubstitute;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.ServicesTests.PredictionStrategiesTests.ConsecutiveNumbersPredictionStrategyTests;

[TestFixture]
public class HandlesTests
{
    private ILotteryConfigurationRepository _configRepo = null!;
    private ILotteryHistoryRepository _historyRepo = null!;
    private ConsecutiveNumbersPredictionStrategy _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _configRepo = Substitute.For<ILotteryConfigurationRepository>();
        _historyRepo = Substitute.For<ILotteryHistoryRepository>();
        _sut = new ConsecutiveNumbersPredictionStrategy(_configRepo, _historyRepo);
    }

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

    [TestCase("consecutive-numbers")]
    [TestCase("CONSECUTIVE-NUMBERS")]
    [TestCase("CoNsEcUtIvE-nUmBeRs")]
    public void Given_A_Valid_Strategy_Should_Return_True(string strategy)
    {
        // Arrange
        // Act
        var result = _sut.Handles(strategy);

        // Assert
        result.Should().BeTrue();
    }
}