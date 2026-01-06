using FluentAssertions;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Lottery.API.Application.Features.GetLotteries;
using JackpotPlot.Lottery.API.Application.Features.GetLotteryConfigurationByLotteryId;
using Lottery.API.Controllers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NUnit.Framework;

namespace Lottery.API.Unit.Tests.ControllersTests.LotteriesControllerTests;

[TestFixture]
public class GetTests
{
    private IMediator _mediator;
    private LotteriesController _sut;

    [SetUp]
    public void SetUp()
    {
        _mediator = Substitute.For<IMediator>();
        _sut = new LotteriesController(_mediator);
    }

    // -------------------------
    // GET api/lotteries
    // -------------------------

    [Test]
    public async Task Given_Success_Result_When_GetAll_Is_Invoked_Should_Return_Ok()
    {
        // Arrange
        var lotteries = new List<LotteryDomain> { new() { Id = 1, Name = "Eurojackpot" } };

        _mediator
            .Send(Arg.Any<GetLotteriesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<ICollection<LotteryDomain>>.Success(lotteries));

        // Act
        var result = await _sut.Get();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Test]
    public async Task Given_Success_Result_When_GetAll_Is_Invoked_Should_Return_Lotteries_As_Ok_Value()
    {
        // Arrange
        var lotteries = new List<LotteryDomain> { new() { Id = 1, Name = "Eurojackpot" } };

        _mediator
            .Send(Arg.Any<GetLotteriesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<ICollection<LotteryDomain>>.Success(lotteries));

        // Act
        var result = await _sut.Get();

        // Assert
        ((OkObjectResult)result).Value.Should().Be(lotteries);
    }

    [Test]
    public async Task Given_Failure_Result_When_GetAll_Is_Invoked_Should_Return_NoContent()
    {
        // Arrange
        _mediator
            .Send(Arg.Any<GetLotteriesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<ICollection<LotteryDomain>>.Failure("failed"));

        // Act
        var result = await _sut.Get();

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Test]
    public async Task Given_Request_When_GetAll_Is_Invoked_Should_Send_GetLotteriesQuery_Once()
    {
        // Arrange
        _mediator
            .Send(Arg.Any<GetLotteriesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<ICollection<LotteryDomain>>.Failure("failed"));

        // Act
        await _sut.Get();

        // Assert
        await _mediator.Received(1).Send(Arg.Any<GetLotteriesQuery>(), Arg.Any<CancellationToken>());
    }

    // -------------------------
    // GET api/lotteries/{id}/configuration
    // -------------------------

    [Test]
    public async Task Given_Success_Result_When_GetConfiguration_Is_Invoked_Should_Return_Ok()
    {
        // Arrange
        const int lotteryId = 7;

        var configuration = CreateConfiguration(lotteryId);

        _mediator
            .Send(Arg.Any<GetLotteryConfigurationByLotteryIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<LotteryConfigurationDomain>.Success(configuration));

        // Act
        var result = await _sut.Get(lotteryId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Test]
    public async Task Given_Success_Result_When_GetConfiguration_Is_Invoked_Should_Return_Configuration_As_Ok_Value()
    {
        // Arrange
        const int lotteryId = 7;

        var configuration = CreateConfiguration(lotteryId);

        _mediator
            .Send(Arg.Any<GetLotteryConfigurationByLotteryIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<LotteryConfigurationDomain>.Success(configuration));

        // Act
        var result = await _sut.Get(lotteryId);

        // Assert
        ((OkObjectResult)result).Value.Should().Be(configuration);
    }

    [Test]
    public async Task Given_Failure_Result_When_GetConfiguration_Is_Invoked_Should_Return_NoContent()
    {
        // Arrange
        const int lotteryId = 7;

        _mediator
            .Send(Arg.Any<GetLotteryConfigurationByLotteryIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<LotteryConfigurationDomain>.Failure("not found"));

        // Act
        var result = await _sut.Get(lotteryId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Test]
    public async Task Given_LotteryId_When_GetConfiguration_Is_Invoked_Should_Send_Query_With_Same_Id()
    {
        // Arrange
        const int lotteryId = 7;

        _mediator
            .Send(Arg.Any<GetLotteryConfigurationByLotteryIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<LotteryConfigurationDomain>.Failure("not found"));

        // Act
        await _sut.Get(lotteryId);

        // Assert
        await _mediator.Received(1).Send(
            Arg.Is<GetLotteryConfigurationByLotteryIdQuery>(q => q.LotteryId == lotteryId),
            Arg.Any<CancellationToken>());
    }

    private static LotteryConfigurationDomain CreateConfiguration(int lotteryId)
    {
        return new LotteryConfigurationDomain
        {
            Id = 1,
            LotteryId = lotteryId,
            DrawType = "standard",
            MainNumbersCount = 5,
            MainNumbersRange = 50,
            BonusNumbersCount = 2,
            BonusNumbersRange = 10,
            DrawFrequency = "weekly"
        };
    }
}