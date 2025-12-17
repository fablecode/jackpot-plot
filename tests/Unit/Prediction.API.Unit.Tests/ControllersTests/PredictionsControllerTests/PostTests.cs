using JackpotPlot.Domain.Models;
using JackpotPlot.Prediction.API.Application.Features.PredictNext;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using Prediction.API.Controllers;
using System.Security.Claims;
using FluentAssertions;
using MediatR;

namespace Prediction.API.Unit.Tests.ControllersTests.PredictionsControllerTests;

[TestFixture]
public class PostTests
{
    private IMediator _mediator;
    private ILogger<PredictionsController> _logger;
    private PredictionsController _sut;

    [SetUp]
    public void SetUp()
    {
        _mediator = Substitute.For<IMediator>();
        _logger = Substitute.For<ILogger<PredictionsController>>();

        _sut = new PredictionsController(_mediator, _logger)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    [Test]
    public async Task Given_Authenticated_User_With_Valid_Guid_Claim_When_Post_Is_Invoked_Should_Send_Request_With_UserId_Set()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetUser(authenticated: true, sub: userId.ToString());

        var request = new PredictNextRequest(LotteryId: 1);

        _mediator
            .Send(Arg.Any<PredictNextRequest>(), Arg.Any<CancellationToken>())
            .Returns(CreateSuccessResult());

        // Act
        await _sut.Post(request);

        // Assert
        await _mediator.Received(1).Send(
            Arg.Is<PredictNextRequest>(r => r.UserId == userId),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Given_Authenticated_User_With_Invalid_Guid_Claim_When_Post_Is_Invoked_Should_Send_Request_With_Null_UserId()
    {
        // Arrange
        SetUser(authenticated: true, sub: "not-a-guid");

        var request = new PredictNextRequest(LotteryId: 1);

        _mediator
            .Send(Arg.Any<PredictNextRequest>(), Arg.Any<CancellationToken>())
            .Returns(CreateSuccessResult());

        // Act
        await _sut.Post(request);

        // Assert
        await _mediator.Received(1).Send(
            Arg.Is<PredictNextRequest>(r => r.UserId == null),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Given_Unauthenticated_User_When_Post_Is_Invoked_Should_Send_Request_With_Null_UserId()
    {
        // Arrange
        SetUser(authenticated: false, sub: Guid.NewGuid().ToString());

        var request = new PredictNextRequest(LotteryId: 1);

        _mediator
            .Send(Arg.Any<PredictNextRequest>(), Arg.Any<CancellationToken>())
            .Returns(CreateSuccessResult());

        // Act
        await _sut.Post(request);

        // Assert
        await _mediator.Received(1).Send(
            Arg.Is<PredictNextRequest>(r => r.UserId == null),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Given_Success_Result_When_Post_Is_Invoked_Should_Return_CreatedAtAction()
    {
        // Arrange
        SetUser(authenticated: false, sub: null);

        var request = new PredictNextRequest(LotteryId: 1);

        _mediator
            .Send(Arg.Any<PredictNextRequest>(), Arg.Any<CancellationToken>())
            .Returns(CreateSuccessResult());

        // Act
        var result = await _sut.Post(request);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Test]
    public async Task Given_Failure_Result_When_Post_Is_Invoked_Should_Return_BadRequest()
    {
        // Arrange
        SetUser(authenticated: false, sub: null);

        var request = new PredictNextRequest(LotteryId: 1);

        _mediator
            .Send(Arg.Any<PredictNextRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result<PredictNextResponse>.Failure("failed"));

        // Act
        var result = await _sut.Post(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Test]
    public async Task Given_Request_When_Post_Is_Invoked_Should_Send_To_Mediator_Once()
    {
        // Arrange
        SetUser(authenticated: false, sub: null);

        var request = new PredictNextRequest(LotteryId: 1);

        _mediator
            .Send(Arg.Any<PredictNextRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result<PredictNextResponse>.Failure("failed"));

        // Act
        await _sut.Post(request);

        // Assert
        await _mediator.Received(1)
            .Send(Arg.Any<PredictNextRequest>(), Arg.Any<CancellationToken>());
    }

    private static Result<PredictNextResponse> CreateSuccessResult()
    {
        var response = new PredictNextResponse(
            LotteryId: 1,
            NumberOfPlays: 1,
            Strategy: "Random",
            Plays: []);

        return Result<PredictNextResponse>.Success(response);
    }

    private void SetUser(bool authenticated, string? sub)
    {
        ClaimsIdentity identity;

        if (authenticated)
        {
            identity = new ClaimsIdentity(
                sub is null ? [] : [new Claim(ClaimTypes.NameIdentifier, sub)],
                authenticationType: "Test");
        }
        else
        {
            identity = new ClaimsIdentity();
        }

        _sut.ControllerContext.HttpContext!.User = new ClaimsPrincipal(identity);
    }
}