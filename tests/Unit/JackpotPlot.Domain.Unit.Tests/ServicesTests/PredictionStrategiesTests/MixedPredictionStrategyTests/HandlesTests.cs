using FluentAssertions;
using JackpotPlot.Domain.Interfaces;
using JackpotPlot.Domain.Repositories;
using JackpotPlot.Domain.Services.PredictionStrategies;
using NSubstitute;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.ServicesTests.PredictionStrategiesTests.MixedPredictionStrategyTests;

[TestFixture]
public class HandlesTests
{
    private MixedPredictionStrategy _sut = null!;

    [SetUp]
    public void SetUp()
    {
        var configRepo = Substitute.For<ILotteryConfigurationRepository>();
        var historyRepo = Substitute.For<ILotteryHistoryRepository>();
        var strategies = new List<IPredictionStrategy>();
        var weights = new Dictionary<string, double>();
        _sut = new MixedPredictionStrategy(strategies, weights, configRepo, historyRepo);
    }

    // ---------- POSITIVE: valid inputs (case-insensitive) ----------

    [TestCase("Mixed")]
    [TestCase("mixed")]
    [TestCase("MIXED")]
    [TestCase("MiXeD")]
    public void Given_Valid_Strategy_When_Handles_Is_Invoked_Should_Return_True(string strategy)
    {
        // Arrange & Act
        var result = _sut.Handles(strategy);

        // Assert
        result.Should().BeTrue();
    }

    // ---------- NEGATIVE: invalid non-null inputs ----------

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