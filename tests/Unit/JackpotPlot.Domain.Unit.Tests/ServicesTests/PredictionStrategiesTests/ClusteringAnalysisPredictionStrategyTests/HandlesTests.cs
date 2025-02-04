using FluentAssertions;
using JackpotPlot.Domain.Repositories;
using JackpotPlot.Domain.Services.PredictionStrategies;
using NSubstitute;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.ServicesTests.PredictionStrategiesTests.ClusteringAnalysisPredictionStrategyTests
{
    [TestFixture]
    public class HandlesTests
    {
        private ClusteringAnalysisPredictionStrategy _sut;

        [SetUp]
        public void SetUp()
        {
            _sut = new ClusteringAnalysisPredictionStrategy(Substitute.For<ILotteryConfigurationRepository>(), Substitute.For<ILotteryHistoryRepository>());
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void Given_An_Invalid_Strategy_Should_Return_False(string strategy)
        {
            // Arrange
            // Act
            var result = _sut.Handles(strategy);

            // Assert
            result.Should().BeFalse();
        }
    }
}
