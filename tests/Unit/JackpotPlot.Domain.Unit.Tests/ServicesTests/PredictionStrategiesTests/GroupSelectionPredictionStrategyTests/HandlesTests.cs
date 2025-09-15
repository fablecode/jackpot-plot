using FluentAssertions;
using JackpotPlot.Domain.Repositories;
using JackpotPlot.Domain.Services.PredictionStrategies;
using NSubstitute;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.ServicesTests.PredictionStrategiesTests.GroupSelectionPredictionStrategyTests;

[TestFixture]
public class HandlesTests
{
    private GroupSelectionPredictionStrategy _sut = null!;

    [SetUp]
    public void SetUp()
    {
        var configRepo = Substitute.For<ILotteryConfigurationRepository>();
        var historyRepo = Substitute.For<ILotteryHistoryRepository>();
        _sut = new GroupSelectionPredictionStrategy(configRepo, historyRepo);
    }

    // ---------- POSITIVE ----------

    [TestCase("Group-Selection")]
    [TestCase("group-selection")]
    [TestCase("GROUP-SELECTION")]
    [TestCase("GrOuP-sElEcTiOn")]
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
    [TestCase("SomeOtherStrategy")]
    public void Given_Invalid_NonNull_Strategy_When_Handles_Is_Invoked_Should_Return_False(string strategy)
    {
        // Arrange & Act
        var result = _sut.Handles(strategy);

        // Assert
        result.Should().BeFalse();
    }
}