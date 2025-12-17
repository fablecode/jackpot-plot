using JackpotPlot.Domain.Models;
using JackpotPlot.Prediction.API.Application.Features.GetPredictionSuccessRate;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using Prediction.API.Controllers;
using System.Collections.Immutable;
using FluentAssertions;
using MediatR;

namespace Prediction.API.Unit.Tests.ControllersTests.PredictionsControllerTests;

[TestFixture]
public class GetPredictionSuccessRateTests
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
    public async Task Given_Success_Result_When_GetPredictionSuccessRate_Is_Invoked_Should_Return_Ok()
    {
        // Arrange
        var output = ImmutableDictionary<int, int>.Empty.Add(1, 42);

        _mediator
            .Send(Arg.Any<GetPredictionSuccessRateQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<ImmutableDictionary<int, int>>.Success(output));

        // Act
        var result = await _sut.GetPredictionSuccessRate();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Test]
    public async Task Given_Success_Result_When_GetPredictionSuccessRate_Is_Invoked_Should_Return_Output_As_Ok_Value()
    {
        // Arrange
        var output = ImmutableDictionary<int, int>.Empty.Add(1, 42);

        _mediator
            .Send(Arg.Any<GetPredictionSuccessRateQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<ImmutableDictionary<int, int>>.Success(output));

        // Act
        var result = await _sut.GetPredictionSuccessRate();

        // Assert
        ((OkObjectResult)result.Result!).Value.Should().Be(output);
    }

    [Test]
    public async Task Given_Failure_Result_When_GetPredictionSuccessRate_Is_Invoked_Should_Return_NoContent()
    {
        // Arrange
        _mediator
            .Send(Arg.Any<GetPredictionSuccessRateQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<ImmutableDictionary<int, int>>.Failure("failed"));

        // Act
        var result = await _sut.GetPredictionSuccessRate();

        // Assert
        result.Result.Should().BeOfType<NoContentResult>();
    }

    [Test]
    public async Task Given_Request_When_GetPredictionSuccessRate_Is_Invoked_Should_Send_Query_Once()
    {
        // Arrange
        _mediator
            .Send(Arg.Any<GetPredictionSuccessRateQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<ImmutableDictionary<int, int>>.Failure("failed"));

        // Act
        await _sut.GetPredictionSuccessRate();

        // Assert
        await _mediator.Received(1).Send(
            Arg.Any<GetPredictionSuccessRateQuery>(),
            Arg.Any<CancellationToken>());
    }
}