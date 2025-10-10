using JackpotPlot.Application.Abstractions.Messaging;
using MassTransit;

namespace JackpotPlot.Infrastructure.Messaging;

public sealed class MassTransitExchangeWriter<T> : IQueueWriter<T>
{
    private readonly IBus _bus;
    private static readonly Uri Exchange = new("exchange:lottery-exchange?type=topic&durable=true");

    public MassTransitExchangeWriter(IBus bus) => _bus = bus;

    public async Task Publish(T message, string routingKey, CancellationToken ct = default)
    {
        var ep = await _bus.GetSendEndpoint(Exchange);
        await ep.Send(message, ctx => ctx.SetRoutingKey(routingKey), ct);
    }
}

