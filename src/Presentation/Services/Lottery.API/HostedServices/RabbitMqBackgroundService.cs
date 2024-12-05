using JackpotPlot.Domain.Messaging;
using MediatR;
using System.Threading.Channels;
using JackpotPlot.Lottery.API.Application.Messaging.Handlers;

namespace Lottery.API.HostedServices;

public abstract class RabbitMqBackgroundService<T> : BackgroundService
{
    protected abstract string QueueName { get; }

    private readonly ILogger<LotteryResultsBackgroundService<T>> _logger;
    private readonly IMediator _mediator;
    private readonly IQueueReader<T> _queueReader;

    protected RabbitMqBackgroundService(ILogger<LotteryResultsBackgroundService<T>> logger, IMediator mediator, IQueueReader<T> queueReader)
    {
        _logger = logger;
        _mediator = mediator;
        _queueReader = queueReader;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("RabbitMq Background Service for {Queue} is starting.", QueueName);

        // Create a channel to receive messages
        var channel = Channel.CreateUnbounded<T>();

        // Start reading messages asynchronously
        var readingTask = _queueReader.Subscribe(QueueName, channel, stoppingToken);

        // Process messages as they come in
        await foreach (var message in channel.Reader.ReadAllAsync(stoppingToken))
        {
            _logger.LogInformation("Message received from queue {Queue}.", QueueName);

            // Process the message (this can be async)
            var result = await _mediator.Send(new MessageHandler<T>(message), stoppingToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Message from queue {QueueName}, processed successfully.", QueueName);
            }
            else
            {
                _logger.LogError("Message from queue {QueueName}, failed with errors. Errors: {Errors}", QueueName, result.Errors);
            }

        }

        // Await the completion of reading messages
        await readingTask;

        _logger.LogInformation("RabbitMq Background Service for {Queue} is stopping.", QueueName);
    }
}