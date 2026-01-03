using JackpotPlot.Domain.Models;
using JackpotPlot.Prediction.API.Application.Features.GetLuckyPair;
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
public class GetLuckPairFrequencyTests
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
    public async Task Given_Success_Result_When_GetLuckPairFrequency_Is_Invoked_Should_Return_Ok()
    {
        // Arrange
        var output = ImmutableArray.Create(new LuckyPairResult(Number1: 1, Number2: 7, Frequency: 42));

        _mediator
            .Send(Arg.Any<GetLuckyPairQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<ImmutableArray<LuckyPairResult>>.Success(output));

        // Act
        var result = await _sut.GetLuckPairFrequency();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Test]
    public async Task Given_Success_Result_When_GetLuckPairFrequency_Is_Invoked_Should_Return_Output_As_Ok_Value()
    {
        // Arrange
        var output = ImmutableArray.Create(new LuckyPairResult(Number1: 1, Number2: 7, Frequency: 42));

        _mediator
            .Send(Arg.Any<GetLuckyPairQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<ImmutableArray<LuckyPairResult>>.Success(output));

        // Act
        var result = await _sut.GetLuckPairFrequency();

        // Assert
        ((OkObjectResult)result.Result!).Value.Should().Be(output);
    }

    [Test]
    public async Task Given_Failure_Result_When_GetLuckPairFrequency_Is_Invoked_Should_Return_NoContent()
    {
        // Arrange
        _mediator
            .Send(Arg.Any<GetLuckyPairQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<ImmutableArray<LuckyPairResult>>.Failure("failed"));

        // Act
        var result = await _sut.GetLuckPairFrequency();

        // Assert
        result.Result.Should().BeOfType<NoContentResult>();
    }

    [Test]
    public async Task Given_Request_When_GetLuckPairFrequency_Is_Invoked_Should_Send_Query_Once()
    {
        // Arrange
        _mediator
            .Send(Arg.Any<GetLuckyPairQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<ImmutableArray<LuckyPairResult>>.Failure("failed"));

        // Act
        await _sut.GetLuckPairFrequency();

        // Assert
        await _mediator.Received(1).Send(Arg.Any<GetLuckyPairQuery>(), Arg.Any<CancellationToken>());
    }
}