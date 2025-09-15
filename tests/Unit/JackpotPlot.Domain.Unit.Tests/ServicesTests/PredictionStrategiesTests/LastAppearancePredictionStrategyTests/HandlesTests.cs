using FluentAssertions;
using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Repositories;
using JackpotPlot.Domain.Services.PredictionStrategies;
using NSubstitute;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.ServicesTests.PredictionStrategiesTests.LastAppearancePredictionStrategyTests;

[TestFixture]
public class HandlesTests
{
    private LastAppearancePredictionStrategy _sut = null!;

    [SetUp]
    public void SetUp()
    {
        var configRepo = Substitute.For<ILotteryConfigurationRepository>();
        var historyRepo = Substitute.For<ILotteryHistoryRepository>();
        _sut = new LastAppearancePredictionStrategy(configRepo, historyRepo);
    }

    // ---------- POSITIVE ----------

    [Test]
    public void Given_Valid_Strategy_When_Handles_Is_Invoked_Should_Return_True()
    {
        // Arrange
        const string strategy = PredictionStrategyType.LastAppearance;

        // Act
        var result = _sut.Handles(strategy);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public void Given_Valid_Strategy_With_Upper_Case_When_Handles_Is_Invoked_Should_Return_True()
    {
        // Arrange
        var strategy = PredictionStrategyType.LastAppearance.ToUpperInvariant();

        // Act
        var result = _sut.Handles(strategy);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public void Given_Valid_Strategy_With_Lower_Case_When_Handles_Is_Invoked_Should_Return_True()
    {
        // Arrange
        var strategy = PredictionStrategyType.LastAppearance.ToLowerInvariant();

        // Act
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