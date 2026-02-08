using NSubstitute;
using NUnit.Framework;
using System.Collections.Immutable;
using FluentAssertions;
using JackpotPlot.Application.Abstractions.Messaging;
using JackpotPlot.Application.Abstractions.Persistence.Repositories;
using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Models;
using JackpotPlot.Lottery.API.Infrastructure.Messaging.Handlers;
using Microsoft.Extensions.Logging;

namespace JackpotPlot.Lottery.API.Infrastructure.Unit.Tests.MessagingTests.HandlersTests;

[TestFixture]
public class EurojackpotResultMessageHandlerTests
{
    private ILogger<EurojackpotResultMessageHandler> _logger;
    private ILotteryRepository _lotteryRepository;
    private IDrawRepository _drawRepository;
    private IDrawResultRepository _drawResultRepository;
    private IQueueWriter<Message<LotteryDrawnEvent>> _queueWriter;

    private EurojackpotResultMessageHandler _sut;

    [SetUp]
    public void SetUp()
    {
        _logger = Substitute.For<ILogger<EurojackpotResultMessageHandler>>();
        _lotteryRepository = Substitute.For<ILotteryRepository>();
        _drawRepository = Substitute.For<IDrawRepository>();
        _drawResultRepository = Substitute.For<IDrawResultRepository>();
        _queueWriter = Substitute.For<IQueueWriter<Message<LotteryDrawnEvent>>>();

        _sut = new EurojackpotResultMessageHandler(
            _logger,
            _lotteryRepository,
            _drawRepository,
            _drawResultRepository,
            _queueWriter);
    }

    [Test]
    public async Task Given_First_Time_When_Handle_Is_Invoked_Should_Return_Success()
    {
        // Arrange
        var request = CreateRequest();
        _lotteryRepository.GetLotteryIdByName("Eurojackpot").Returns(7);

        _drawRepository
            .DrawExist(7, request.Message.Data.Date, request.Message.Data.MainNumbers, request.Message.Data.EuroNumbers)
            .Returns(false);

        _drawRepository.Add(7, request.Message.Data).Returns(123);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task Given_First_Time_When_Handle_Is_Invoked_Should_Add_Draw_Once()
    {
        // Arrange
        var request = CreateRequest();
        _lotteryRepository.GetLotteryIdByName("Eurojackpot").Returns(7);

        _drawRepository
            .DrawExist(7, request.Message.Data.Date, request.Message.Data.MainNumbers, request.Message.Data.EuroNumbers)
            .Returns(false);

        _drawRepository.Add(7, request.Message.Data).Returns(123);

        // Act
        await _sut.Handle(request, CancellationToken.None);

        // Assert
        await _drawRepository.Received(1).Add(7, request.Message.Data);
    }

    [Test]
    public async Task Given_First_Time_When_Handle_Is_Invoked_Should_Add_Draw_Result_Once()
    {
        // Arrange
        var request = CreateRequest();
        _lotteryRepository.GetLotteryIdByName("Eurojackpot").Returns(7);

        _drawRepository
            .DrawExist(7, request.Message.Data.Date, request.Message.Data.MainNumbers, request.Message.Data.EuroNumbers)
            .Returns(false);

        _drawRepository.Add(7, request.Message.Data).Returns(123);

        // Act
        await _sut.Handle(request, CancellationToken.None);

        // Assert
        await _drawResultRepository.Received(1).Add(123, request.Message.Data);
    }

    [Test]
    public async Task Given_First_Time_When_Handle_Is_Invoked_Should_Publish_Once()
    {
        // Arrange
        var request = CreateRequest();
        _lotteryRepository.GetLotteryIdByName("Eurojackpot").Returns(7);

        _drawRepository
            .DrawExist(7, request.Message.Data.Date, request.Message.Data.MainNumbers, request.Message.Data.EuroNumbers)
            .Returns(false);

        _drawRepository.Add(7, request.Message.Data).Returns(123);

        // Act
        await _sut.Handle(request, CancellationToken.None);

        // Assert
        await _queueWriter.Received(1).Publish(
            Arg.Any<Message<LotteryDrawnEvent>>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Given_First_Time_When_Handle_Is_Invoked_Should_Publish_With_Expected_RoutingKey()
    {
        // Arrange
        var request = CreateRequest();
        _lotteryRepository.GetLotteryIdByName("Eurojackpot").Returns(7);

        _drawRepository
            .DrawExist(7, request.Message.Data.Date, request.Message.Data.MainNumbers, request.Message.Data.EuroNumbers)
            .Returns(false);

        _drawRepository.Add(7, request.Message.Data).Returns(123);

        var expectedRoutingKey = $"{RoutingKeys.LotteryDbUpdate}.{EventTypes.LotteryDrawn}";

        // Act
        await _sut.Handle(request, CancellationToken.None);

        // Assert
        await _queueWriter.Received(1).Publish(
            Arg.Any<Message<LotteryDrawnEvent>>(),
            Arg.Is<string>(rk => rk == expectedRoutingKey),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Given_First_Time_When_Handle_Is_Invoked_Should_Publish_LotteryDrawn_EventType()
    {
        // Arrange
        var request = CreateRequest();
        _lotteryRepository.GetLotteryIdByName("Eurojackpot").Returns(7);

        _drawRepository
            .DrawExist(7, request.Message.Data.Date, request.Message.Data.MainNumbers, request.Message.Data.EuroNumbers)
            .Returns(false);

        _drawRepository.Add(7, request.Message.Data).Returns(123);

        // Act
        await _sut.Handle(request, CancellationToken.None);

        // Assert
        await _queueWriter.Received(1).Publish(
            Arg.Is<Message<LotteryDrawnEvent>>(m => m.Event == EventTypes.LotteryDrawn),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Given_First_Time_When_Handle_Is_Invoked_Should_Publish_Event_With_Mapped_LotteryId()
    {
        // Arrange
        var request = CreateRequest();
        _lotteryRepository.GetLotteryIdByName("Eurojackpot").Returns(7);

        _drawRepository
            .DrawExist(7, request.Message.Data.Date, request.Message.Data.MainNumbers, request.Message.Data.EuroNumbers)
            .Returns(false);

        _drawRepository.Add(7, request.Message.Data).Returns(123);

        // Act
        await _sut.Handle(request, CancellationToken.None);

        // Assert
        await _queueWriter.Received(1).Publish(
            Arg.Is<Message<LotteryDrawnEvent>>(m => m.Data.LotteryId == 7),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Given_Duplicate_Draw_When_Handle_Is_Invoked_Should_Not_Publish()
    {
        // Arrange
        var request = CreateRequest();
        _lotteryRepository.GetLotteryIdByName("Eurojackpot").Returns(7);

        _drawRepository
            .DrawExist(7, request.Message.Data.Date, request.Message.Data.MainNumbers, request.Message.Data.EuroNumbers)
            .Returns(true);

        // Act
        await _sut.Handle(request, CancellationToken.None);

        // Assert
        await _queueWriter.DidNotReceive().Publish(
            Arg.Any<Message<LotteryDrawnEvent>>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Given_Duplicate_Draw_When_Handle_Is_Invoked_Should_Return_Success()
    {
        // Arrange
        var request = CreateRequest();
        _lotteryRepository.GetLotteryIdByName("Eurojackpot").Returns(7);

        _drawRepository
            .DrawExist(7, request.Message.Data.Date, request.Message.Data.MainNumbers, request.Message.Data.EuroNumbers)
            .Returns(true);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task Given_Two_Invocations_When_Handle_Is_Invoked_Should_Resolve_LotteryId_Only_Once()
    {
        // Arrange
        var request = CreateRequest();
        _lotteryRepository.GetLotteryIdByName("Eurojackpot").Returns(7);

        _drawRepository
            .DrawExist(7, request.Message.Data.Date, request.Message.Data.MainNumbers, request.Message.Data.EuroNumbers)
            .Returns(true);

        // Act
        await _sut.Handle(request, CancellationToken.None);
        await _sut.Handle(request, CancellationToken.None);

        // Assert
        await _lotteryRepository.Received(1).GetLotteryIdByName("Eurojackpot");
    }

    private static JackpotPlot.Infrastructure.Messaging.MessageHandler<Message<EurojackpotResult>> CreateRequest()
    {
        var result = new EurojackpotResult
        {
            Date = new DateTime(2026, 02, 06),
            MainNumbers = ImmutableArray.Create(1, 2, 3, 4, 5),
            EuroNumbers = ImmutableArray.Create(6, 7)
        };

        var message = new Message<EurojackpotResult>(EventTypes.EurojackpotDraw, result);
        return new JackpotPlot.Infrastructure.Messaging.MessageHandler<Message<EurojackpotResult>>(message);
    }
}