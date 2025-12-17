using FluentAssertions;
using JackpotPlot.Domain.Models;
using JackpotPlot.Prediction.API.Application.Features.GetHotAndColdNumbersByLotteryId;
using JackpotPlot.Prediction.API.Application.Models.Output;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using Prediction.API.Controllers;

namespace Prediction.API.Unit.Tests.ControllersTests.PredictionsControllerTests;

[TestFixture]
public class GetHotColdNumbersTests
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
    public async Task Given_Success_Result_When_GetHotColdNumbers_Is_Invoked_Should_Return_Ok()
    {
        // Arrange
        const int lotteryId = 7;

        var output = new HotColdNumbersOutput(
            HotNumbers: new Dictionary<int, int> { [1] = 10 },
            ColdNumbers: new Dictionary<int, int> { [2] = 1 });

        _mediator
            .Send(Arg.Any<GetHotAndColdNumbersByLotteryIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<HotColdNumbersOutput>.Success(output));

        // Act
        var result = await _sut.GetHotColdNumbers(lotteryId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Test]
    public async Task Given_Success_Result_When_GetHotColdNumbers_Is_Invoked_Should_Return_Output_As_Ok_Value()
    {
        // Arrange
        const int lotteryId = 7;

        var output = new HotColdNumbersOutput(
            HotNumbers: new Dictionary<int, int> { [1] = 10 },
            ColdNumbers: new Dictionary<int, int> { [2] = 1 });

        _mediator
            .Send(Arg.Any<GetHotAndColdNumbersByLotteryIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<HotColdNumbersOutput>.Success(output));

        // Act
        var result = await _sut.GetHotColdNumbers(lotteryId);

        // Assert
        ((OkObjectResult)result).Value.Should().Be(output);
    }

    [Test]
    public async Task Given_Failure_Result_When_GetHotColdNumbers_Is_Invoked_Should_Return_NoContent()
    {
        // Arrange
        const int lotteryId = 7;

        _mediator
            .Send(Arg.Any<GetHotAndColdNumbersByLotteryIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<HotColdNumbersOutput>.Failure("failed"));

        // Act
        var result = await _sut.GetHotColdNumbers(lotteryId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Test]
    public async Task Given_LotteryId_When_GetHotColdNumbers_Is_Invoked_Should_Send_Query_With_Same_LotteryId()
    {
        // Arrange
        const int lotteryId = 7;

        _mediator
            .Send(Arg.Any<GetHotAndColdNumbersByLotteryIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<HotColdNumbersOutput>.Failure("failed"));

        // Act
        await _sut.GetHotColdNumbers(lotteryId);

        // Assert
        await _mediator.Received(1).Send(
            Arg.Is<GetHotAndColdNumbersByLotteryIdQuery>(q => q.LotteryId == lotteryId),
            Arg.Any<CancellationToken>());
    }
}