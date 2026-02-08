using FluentAssertions;
using JackpotPlot.Domain.Models;
using JackpotPlot.Infrastructure.Messaging;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

namespace JackpotPlot.Infrastructure.Unit.Tests.MessagingTests.QueueToMediatorConsumerTests;

[TestFixture]
public class ConsumeTests
{
    private ILogger<QueueToMediatorConsumer<string>> _logger;
    private IMediator _mediator;
    private QueueToMediatorConsumer<string> _sut;

    [SetUp]
    public void SetUp()
    {
        _logger = Substitute.For<ILogger<QueueToMediatorConsumer<string>>>();
        _mediator = Substitute.For<IMediator>();
        _sut = new QueueToMediatorConsumer<string>(_logger, _mediator);
    }

    [Test]
    public async Task Given_Valid_Message_When_Consume_Is_Invoked_Should_Send_MessageHandler_To_Mediator_Once()
    {
        // Arrange
        var context = Substitute.For<ConsumeContext<string>>();
        context.Message.Returns("hello");
        context.CancellationToken.Returns(CancellationToken.None);

        _mediator
            .Send(Arg.Any<Messaging.MessageHandler<string>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<string>.Success("hello")));

        // Act
        await _sut.Consume(context);

        // Assert
        await _mediator.Received(1).Send(
            Arg.Any<Messaging.MessageHandler<string>>(),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Given_Message_When_Consume_Is_Invoked_Should_Send_MessageHandler_With_Same_Message()
    {
        // Arrange
        const string message = "hello";
        var context = Substitute.For<ConsumeContext<string>>();
        context.Message.Returns(message);
        context.CancellationToken.Returns(CancellationToken.None);

        _mediator
            .Send(Arg.Any<Messaging.MessageHandler<string>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<string>.Success(message)));

        // Act
        await _sut.Consume(context);

        // Assert
        await _mediator.Received(1).Send(
            Arg.Is<Messaging.MessageHandler<string>>(r => r.Message == message),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Given_CancellationToken_When_Consume_Is_Invoked_Should_Pass_Token_To_Mediator()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var ct = cts.Token;

        var context = Substitute.For<ConsumeContext<string>>();
        context.Message.Returns("hello");
        context.CancellationToken.Returns(ct);

        _mediator
            .Send(Arg.Any<Messaging.MessageHandler<string>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<string>.Success("hello")));

        // Act
        await _sut.Consume(context);

        // Assert
        await _mediator.Received(1).Send(
            Arg.Any<Messaging.MessageHandler<string>>(),
            Arg.Is<CancellationToken>(t => t == ct));
    }

    [Test]
    public void Given_Handler_Failure_Result_When_Consume_Is_Invoked_Should_Throw_InvalidOperationException()
    {
        // Arrange
        var context = Substitute.For<ConsumeContext<string>>();
        context.Message.Returns("hello");
        context.CancellationToken.Returns(CancellationToken.None);

        _mediator
            .Send(Arg.Any<Messaging.MessageHandler<string>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<string>.Failure("nope")));

        // Act
        Func<Task> act = async () => await _sut.Consume(context);

        // Assert
        act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Test]
    public void Given_Mediator_Throws_When_Consume_Is_Invoked_Should_Propagate_Exception()
    {
        // Arrange
        var context = Substitute.For<ConsumeContext<string>>();
        context.Message.Returns("hello");
        context.CancellationToken.Returns(CancellationToken.None);

        _mediator
            .Send(Arg.Any<Messaging.MessageHandler<string>>(), Arg.Any<CancellationToken>())
            .Returns<Task<Result<string>>>(_ => throw new InvalidOperationException("boom"));

        // Act
        Func<Task> act = async () => await _sut.Consume(context);

        // Assert
        act.Should().ThrowAsync<InvalidOperationException>();
    }
}