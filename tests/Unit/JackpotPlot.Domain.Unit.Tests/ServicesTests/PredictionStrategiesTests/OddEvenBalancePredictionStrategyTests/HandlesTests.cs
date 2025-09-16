using FluentAssertions;
using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Repositories;
using JackpotPlot.Domain.Services.PredictionStrategies;
using NSubstitute;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.ServicesTests.PredictionStrategiesTests.OddEvenBalancePredictionStrategyTests;

[TestFixture]
public class HandlesTests
{
    private OddEvenBalancePredictionStrategy _sut = null!;

    [SetUp]
    public void SetUp()
    {
        var configRepo = Substitute.For<ILotteryConfigurationRepository>();
        var historyRepo = Substitute.For<ILotteryHistoryRepository>();
        _sut = new OddEvenBalancePredictionStrategy(configRepo, historyRepo);
    }

    // ---------- POSITIVE (case-insensitive matches) ----------

    [Test]
    public void Given_Exact_Strategy_When_Handles_Is_Invoked_Should_Return_True()
    {
        // Arrange
        var strategy = PredictionStrategyType.OddEvenBalance;

        // Act
        var result = _sut.Handles(strategy);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public void Given_UpperCase_Strategy_When_Handles_Is_Invoked_Should_Return_True()
    {
        // Arrange
        var strategy = PredictionStrategyType.OddEvenBalance.ToUpperInvariant();

        // Act
        var result = _sut.Handles(strategy);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public void Given_LowerCase_Strategy_When_Handles_Is_Invoked_Should_Return_True()
    {
        // Arrange
        var strategy = PredictionStrategyType.OddEvenBalance.ToLowerInvariant();

        // Act
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
    public void Given_Invalid_NonNull_Strategy_When_Handles_Is_Invoked_Should_Return_False(string strategy)
    {
        // Arrange & Act
        var result = _sut.Handles(strategy);

        // Assert
        result.Should().BeFalse();
    }
}