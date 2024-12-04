using JackpotPlot.Domain.Messaging;
using System.Text.Json;
using System.Threading.Channels;

namespace Lottery.API.HostedServices;

public class LotteryResultBackgroundService<T> : BackgroundService
{
    private readonly ILogger<LotteryResultBackgroundService<T>> _logger;
    private readonly IQueueReader<T> _queueReader;

    public LotteryResultBackgroundService(ILogger<LotteryResultBackgroundService<T>> logger, IQueueReader<T> queueReader)
    {
        _logger = logger;
        _queueReader = queueReader;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("RabbitMQ Background Service is starting.");

        // Create a channel to receive messages
        var channel = Channel.CreateUnbounded<T>();

        // Start reading messages asynchronously
        var readingTask = _queueReader.Subscribe("lottery-results", channel, stoppingToken);

        // Process messages as they come in
        await foreach (var message in channel.Reader.ReadAllAsync(stoppingToken))
        {
            _logger.LogInformation($"Message received: {JsonSerializer.Serialize(message)}");

            // Process the message (this can be async)
            await Task.Delay(1000, stoppingToken); // Simulate some processing work
        }

        // Await the completion of reading messages
        await readingTask;

        _logger.LogInformation("RabbitMQ Background Service is stopping.");
    }
}
