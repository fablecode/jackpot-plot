using FluentAssertions;
using JackpotPlot.Domain.Models;
using JackpotPlot.Prediction.API.Application.Features.GetHotAndColdNumbers;
using JackpotPlot.Prediction.API.Application.Models.Output;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NUnit.Framework;
using Prediction.API.Controllers;

namespace Prediction.API.Unit.Tests.ControllersTests.LotteriesControllerTests;

[TestFixture]
public class GetHotAndColdNumbersTests
{
    private IMediator _mediator;
    private LotteriesController _sut;

    [SetUp]
    public void SetUp()
    {
        _mediator = Substitute.For<IMediator>();
        _sut = new LotteriesController(_mediator);
    }

    [Test]
    public async Task Given_Success_Result_When_GetHotAndColdNumbers_Is_Invoked_Should_Return_Ok()
    {
        // Arrange
        var output = new HotColdNumbersOutput(
            HotNumbers: new Dictionary<int, int> { [1] = 10 },
            ColdNumbers: new Dictionary<int, int> { [2] = 1 });

        _mediator
            .Send(Arg.Any<GetHotAndColdNumbersQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<HotColdNumbersOutput>.Success(output));

        // Act
        var result = await _sut.GetHotAndColdNumbers();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Test]
    public async Task Given_Success_Result_When_GetHotAndColdNumbers_Is_Invoked_Should_Return_Output_As_Ok_Value()
    {
        // Arrange
        var output = new HotColdNumbersOutput(
            HotNumbers: new Dictionary<int, int> { [1] = 10 },
            ColdNumbers: new Dictionary<int, int> { [2] = 1 });

        _mediator
            .Send(Arg.Any<GetHotAndColdNumbersQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<HotColdNumbersOutput>.Success(output));

        // Act
        var result = await _sut.GetHotAndColdNumbers();

        // Assert
        ((OkObjectResult)result).Value.Should().Be(output);
    }

    [Test]
    public async Task Given_Failure_Result_When_GetHotAndColdNumbers_Is_Invoked_Should_Return_NoContent()
    {
        // Arrange
        _mediator
            .Send(Arg.Any<GetHotAndColdNumbersQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<HotColdNumbersOutput>.Failure("failed"));

        // Act
        var result = await _sut.GetHotAndColdNumbers();

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Test]
    public async Task Given_Request_When_GetHotAndColdNumbers_Is_Invoked_Should_Send_Query_Once()
    {
        // Arrange
        _mediator
            .Send(Arg.Any<GetHotAndColdNumbersQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<HotColdNumbersOutput>.Failure("failed"));

        // Act
        await _sut.GetHotAndColdNumbers();

        // Assert
        await _mediator.Received(1).Send(Arg.Any<GetHotAndColdNumbersQuery>(), Arg.Any<CancellationToken>());
    }
}