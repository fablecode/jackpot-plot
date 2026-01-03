using FluentAssertions;
using JackpotPlot.Domain.Models;
using JackpotPlot.Prediction.API.Application.Features.GetNumberSpread;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using Prediction.API.Controllers;

namespace Prediction.API.Unit.Tests.ControllersTests.PredictionsControllerTests;

[TestFixture]
public class GetNumberSpreadTests
{
    private IMediator _mediator;
    private ILogger<PredictionsController> _logger;
    private PredictionsController _sut;

    [SetUp]
    public void SetUp()
    {
        _mediator = Substitute.For<IMediator>();
        _logger = Substitute.For<ILogger<PredictionsController>>();
        _sut = new PredictionsController(_mediator, _logger);
    }

    [Test]
    public async Task Given_Success_Result_When_GetNumberSpread_Is_Invoked_Should_Return_Ok()
    {
        // Arrange
        var output = new NumberSpreadResult(Low: 3, Mid: 10, High: 17);

        _mediator
            .Send(Arg.Any<GetNumberSpreadQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<NumberSpreadResult>.Success(output));

        // Act
        var result = await _sut.GetNumberSpread();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Test]
    public async Task Given_Success_Result_When_GetNumberSpread_Is_Invoked_Should_Return_Output_As_Ok_Value()
    {
        // Arrange
        var output = new NumberSpreadResult(Low: 3, Mid: 10, High: 17);

        _mediator
            .Send(Arg.Any<GetNumberSpreadQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<NumberSpreadResult>.Success(output));

        // Act
        var result = await _sut.GetNumberSpread();

        // Assert
        ((OkObjectResult)result.Result!).Value.Should().Be(output);
    }

    [Test]
    public async Task Given_Failure_Result_When_GetNumberSpread_Is_Invoked_Should_Return_NoContent()
    {
        // Arrange
        _mediator
            .Send(Arg.Any<GetNumberSpreadQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<NumberSpreadResult>.Failure("failed"));

        // Act
        var result = await _sut.GetNumberSpread();

        // Assert
        result.Result.Should().BeOfType<NoContentResult>();
    }

    [Test]
    public async Task Given_Request_When_GetNumberSpread_Is_Invoked_Should_Send_Query_Once()
    {
        // Arrange
        _mediator
            .Send(Arg.Any<GetNumberSpreadQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<NumberSpreadResult>.Failure("failed"));

        // Act
        await _sut.GetNumberSpread();

        // Assert
        await _mediator.Received(1)
            .Send(Arg.Any<GetNumberSpreadQuery>(), Arg.Any<CancellationToken>());
    }
}