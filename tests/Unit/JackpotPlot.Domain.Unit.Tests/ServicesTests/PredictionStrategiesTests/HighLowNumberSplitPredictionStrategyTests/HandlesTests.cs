using FluentAssertions;
using JackpotPlot.Domain.Repositories;
using JackpotPlot.Domain.Services.PredictionStrategies;
using NSubstitute;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.ServicesTests.PredictionStrategiesTests.HighLowNumberSplitPredictionStrategyTests;

[TestFixture]
public class HandlesTests
{
    private HighLowNumberSplitPredictionStrategy _sut = null!;

    [SetUp]
    public void SetUp()
    {
        var configRepo = Substitute.For<ILotteryConfigurationRepository>();
        var historyRepo = Substitute.For<ILotteryHistoryRepository>();
        _sut = new HighLowNumberSplitPredictionStrategy(configRepo, historyRepo);
    }

    // ---------- POSITIVE ----------

    [TestCase("High-Low-Number-Split")]
    [TestCase("high-low-number-split")]
    [TestCase("HIGH-LOW-NUMBER-SPLIT")]
    [TestCase("HiGh-LoW-nUmBeR-sPlIt")]
    public void Given_Valid_Strategy_When_Handles_Is_Invoked_Should_Return_True(string strategy)
    {
        // Arrange & Act
        var result = _sut.Handles(strategy);

        // Assert
        result.Should().BeTrue();
    }

    // ---------- NEGATIVE (non-null invalid inputs) ----------

    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    [TestCase("\t")]
    [TestCase("SomeOtherStrategy")]
    public void Given_Invalid_NonNull_Strategy_When_Handles_Is_Invoked_Should_Return_False(string strategy)
    {
        // Arrange & Act
        var result = _sut.Handles(strategy);

        // Assert
        result.Should().BeFalse();
    }
}