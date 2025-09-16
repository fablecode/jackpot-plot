using FluentAssertions;
using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Repositories;
using JackpotPlot.Domain.Services.PredictionStrategies;
using NSubstitute;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.ServicesTests.PredictionStrategiesTests.NumberSumPredictionStrategyTests;

[TestFixture]
public class HandlesTests
{
    private NumberSumPredictionStrategy _sut = null!;

    [SetUp]
    public void SetUp()
    {
        var configRepo = Substitute.For<ILotteryConfigurationRepository>();
        var historyRepo = Substitute.For<ILotteryHistoryRepository>();
        _sut = new NumberSumPredictionStrategy(configRepo, historyRepo);
    }

    // ---------- POSITIVE: valid inputs (case-insensitive) ----------

    [TestCase("Number-Sum")]
    [TestCase("number-sum")]
    [TestCase("NUMBER-SUM")]
    [TestCase("NuMbEr-SuM")]
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