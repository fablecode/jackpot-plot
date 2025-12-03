using FluentAssertions;
using JackpotPlot.Application.Abstractions.Persistence.Repositories;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Lottery.API.Application.Features.GetLotteryConfigurationByLotteryId;
using NSubstitute;
using NUnit.Framework;

namespace JackpotPlot.Lottery.API.Application.Unit.Tests.FeaturesTests.GetLotteryConfigurationByLotteryIdHandlerTests;

[TestFixture]
public class HandleTests
{
    private ILotteryConfigurationRepository _lotteryConfigurationRepository = null!;
    private GetLotteryConfigurationByLotteryIdHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _lotteryConfigurationRepository = Substitute.For<ILotteryConfigurationRepository>();
        _sut = new GetLotteryConfigurationByLotteryIdHandler(_lotteryConfigurationRepository);
    }

    [Test]
    public async Task Given_Query_When_Handle_Is_Invoked_Should_Call_Repository_With_LotteryId()
    {
        // Arrange
        const int lotteryId = 42;
        _lotteryConfigurationRepository
            .GetActiveConfigurationAsync(lotteryId)
            .Returns((LotteryConfigurationDomain?)null);

        var query = new GetLotteryConfigurationByLotteryIdQuery(lotteryId);

        // Act
        _ = await _sut.Handle(query, CancellationToken.None);

        // Assert
        await _lotteryConfigurationRepository.Received(1)
            .GetActiveConfigurationAsync(lotteryId);
    }

    [Test]
    public async Task Given_Configuration_Exists_When_Handle_Is_Invoked_Should_Return_Success_Result()
    {
        // Arrange
        const int lotteryId = 7;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId };

        _lotteryConfigurationRepository
            .GetActiveConfigurationAsync(lotteryId)
            .Returns(config);

        var query = new GetLotteryConfigurationByLotteryIdQuery(lotteryId);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task Given_Configuration_Exists_When_Handle_Is_Invoked_Should_Return_Configuration_From_Repository()
    {
        // Arrange
        const int lotteryId = 5;
        var config = new LotteryConfigurationDomain { LotteryId = lotteryId };

        _lotteryConfigurationRepository
            .GetActiveConfigurationAsync(lotteryId)
            .Returns(config);

        var query = new GetLotteryConfigurationByLotteryIdQuery(lotteryId);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Value.LotteryId.Should().Be(lotteryId);
    }

    [Test]
    public async Task Given_Configuration_Not_Found_When_Handle_Is_Invoked_Should_Return_Failure_Result()
    {
        // Arrange
        const int lotteryId = 10;

        _lotteryConfigurationRepository
            .GetActiveConfigurationAsync(lotteryId)
            .Returns((LotteryConfigurationDomain?)null);

        var query = new GetLotteryConfigurationByLotteryIdQuery(lotteryId);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Test]
    public async Task Given_Configuration_Not_Found_When_Handle_Is_Invoked_Should_Contain_LotteryId_In_Error_Message()
    {
        // Arrange
        const int lotteryId = 13;

        _lotteryConfigurationRepository
            .GetActiveConfigurationAsync(lotteryId)
            .Returns((LotteryConfigurationDomain?)null);

        var query = new GetLotteryConfigurationByLotteryIdQuery(lotteryId);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Errors.Should().Contain(e => e.Contains(lotteryId.ToString()));
    }
}