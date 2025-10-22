using JackpotPlot.Application.Abstractions.Messaging;
using JackpotPlot.Domain.Settings;
using MassTransit;
using Microsoft.Extensions.Options;

namespace JackpotPlot.Infrastructure.Messaging;

public sealed class MassTransitExchangeWriter<T> : IQueueWriter<T>
{
    private readonly IBus _bus;
    private readonly IOptions<RabbitMqSettings> _options;

    public MassTransitExchangeWriter(IBus bus, IOptions<RabbitMqSettings> options)
    {
        _bus = bus;
        _options = options;
    }

    public async Task Publish(T message, string routingKey, CancellationToken ct = default)
    {
        var ep = await _bus.GetSendEndpoint(new($"exchange:{_options.Value.Exchange}?type=topic&durable=true"));

        await ep.Send(message ?? throw new ArgumentNullException(nameof(message)), ctx => ctx.SetRoutingKey(routingKey), ct);
    }
}

