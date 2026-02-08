using FluentAssertions;
using JackpotPlot.Domain.Settings;
using JackpotPlot.Infrastructure.Messaging;
using MassTransit;
using Microsoft.Extensions.Options;
using NSubstitute;
using NUnit.Framework;

namespace JackpotPlot.Infrastructure.Unit.Tests.MessagingTests.MassTransitExchangeWriterTests;

[TestFixture]
public class PublishTests
{
    private IBus _bus;
    private ISendEndpoint _sendEndpoint;
    private IOptions<RabbitMqSettings> _options;

    [SetUp]
    public void SetUp()
    {
        _bus = Substitute.For<IBus>();
        _sendEndpoint = Substitute.For<ISendEndpoint>();

        _options = Options.Create(new RabbitMqSettings
        {
            Host = "localhost",
            Username = "guest",
            Password = "guest",
            Exchange = "jackpot-plot-exchange"
        });

        _bus.GetSendEndpoint(Arg.Any<Uri>())
            .Returns(Task.FromResult(_sendEndpoint));
    }

    [Test]
    public void Given_Null_Message_When_Publish_Is_Invoked_Should_Throw_ArgumentNullException()
    {
        // Arrange
        var sut = new MassTransitExchangeWriter<string>(_bus, _options);

        // Act
        Func<Task> act = async () => await sut.Publish(null!, routingKey: "rk", CancellationToken.None);

        // Assert
        act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Test]
    public async Task Given_Valid_Message_When_Publish_Is_Invoked_Should_Request_SendEndpoint_With_Expected_Exchange_Uri()
    {
        // Arrange
        var sut = new MassTransitExchangeWriter<string>(_bus, _options);

        // Act
        await sut.Publish("hello", routingKey: "rk", CancellationToken.None);

        // Assert
        await _bus.Received(1).GetSendEndpoint(
            Arg.Is<Uri>(u => u.ToString() == "exchange:jackpot-plot-exchange?type=topic&durable=true"));
    }

    [Test]
    public async Task Given_Valid_Message_When_Publish_Is_Invoked_Should_Send_Message_Once()
    {
        // Arrange
        var sut = new MassTransitExchangeWriter<string>(_bus, _options);
        const string message = "hello";

        // Act
        await sut.Publish(message, routingKey: "rk", CancellationToken.None);

        // Assert
        _sendEndpoint.ReceivedCalls()
            .Count(c => c.GetMethodInfo().Name == "Send"
                        && c.GetArguments().Length > 0
                        && c.GetArguments()[0] is string s
                        && s == message)
            .Should().Be(1);
    }

    [Test]
    public async Task Given_CancellationToken_When_Publish_Is_Invoked_Should_Pass_Token_To_Send()
    {
        // Arrange
        var sut = new MassTransitExchangeWriter<string>(_bus, _options);
        using var cts = new CancellationTokenSource();
        var ct = cts.Token;

        // Act
        await sut.Publish("hello", routingKey: "rk", ct);

        // Assert
        _sendEndpoint.ReceivedCalls()
            .Count(c =>
                c.GetMethodInfo().Name == "Send"
                && c.GetArguments().Length > 0
                && c.GetArguments().Last() is CancellationToken token
                && token == ct)
            .Should().Be(1);
    }
}