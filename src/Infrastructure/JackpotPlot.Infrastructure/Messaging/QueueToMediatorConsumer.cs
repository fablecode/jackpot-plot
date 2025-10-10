using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace JackpotPlot.Infrastructure.Messaging;

public sealed class QueueToMediatorConsumer<T> : IConsumer<T> where T : class
{
    private readonly ILogger<QueueToMediatorConsumer<T>> _logger;
    private readonly IMediator _mediator;

    public QueueToMediatorConsumer(ILogger<QueueToMediatorConsumer<T>> logger, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    public async Task Consume(ConsumeContext<T> context)
    {
        _logger.LogInformation("Consuming {MessageType} via MediatR", typeof(T).Name);

        // Reuse your existing MediatR request wrapper
        var result = await _mediator.Send(new MessageHandler<T>(context.Message), context.CancellationToken);

        if (!result.IsSuccess)
        {
            _logger.LogError("MediatR handler failed for {Type}: {Errors}", typeof(T).Name, string.Join("; ", result.Errors));
            // Throw to trigger MassTransit retries and move to _error after policy exhausted
            throw new InvalidOperationException("Handler reported failure.");
        }

        _logger.LogInformation("{MessageType} processed successfully", typeof(T).Name);
    }
}