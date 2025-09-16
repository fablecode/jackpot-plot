using FluentAssertions;
using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Repositories;
using JackpotPlot.Domain.Services.PredictionStrategies;
using NSubstitute;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.ServicesTests.PredictionStrategiesTests.NumberChainPredictionStrategyTests;

[TestFixture]
public class HandlesTests
{
    private NumberChainPredictionStrategy _sut = null!;

    [SetUp]
    public void SetUp()
    {
        var configRepo = Substitute.For<ILotteryConfigurationRepository>();
        var historyRepo = Substitute.For<ILotteryHistoryRepository>();
        _sut = new NumberChainPredictionStrategy(configRepo, historyRepo);
    }

    // ---------- POSITIVE (case-insensitive matches) ----------

    [TestCase("Number-Chain")]
    [TestCase("number-chain")]
    [TestCase("NUMBER-CHAIN")]
    [TestCase("NuMbEr-ChAiN")]
    public void Given_Valid_Strategy_When_Handles_Is_Invoked_Should_Return_True(string strategy)
    {
        // Arrange & Act
        var result = _sut.Handles(strategy);

        // Assert
        result.Should().BeTrue();
    }

    // ---------- NEGATIVE (invalid non-null inputs) ----------

    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    [TestCase("\t")]
    [TestCase("SomeOtherStrategy")]
    [TestCase(PredictionStrategyType.FrequencyBased)]
    public void Given_Invalid_NonNull_Strategy_When_Handles_Is_Invoked_Should_Return_False(string strategy)
    {
        // Arrange & Act
        var result = _sut.Handles(strategy);

        // Assert
        result.Should().BeFalse();
    }
}