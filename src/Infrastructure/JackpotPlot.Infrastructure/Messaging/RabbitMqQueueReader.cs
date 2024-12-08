using System.Text;
using System.Threading.Channels;
using JackpotPlot.Domain.Messaging;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace JackpotPlot.Infrastructure.Messaging;

public class RabbitMqQueueReader<T> : IQueueReader<T>
{
    private readonly ILogger<RabbitMqQueueReader<T>> _logger;
    private readonly ConnectionFactory _factory;

    public RabbitMqQueueReader(ILogger<RabbitMqQueueReader<T>> logger, IOptions<RabbitMqSettings> rabbitMqConfig)
    {
        _logger = logger;
        _factory = new ConnectionFactory()
        {
            HostName = rabbitMqConfig.Value.Host, 
            Port = rabbitMqConfig.Value.Port, 
            UserName = rabbitMqConfig.Value.Username, 
            Password = rabbitMqConfig.Value.Password
        };
    }
    public async ValueTask Subscribe(string queueName, Channel<(T message, TaskCompletionSource<Result<T>> tcs)> channel, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting subscription to queue: {QueueName}", queueName);

        await using var rabbitMqConnection = await _factory.CreateConnectionAsync(cancellationToken);
        await using var rabbitMqChannel = await rabbitMqConnection.CreateChannelAsync(cancellationToken: cancellationToken);

        // Set QoS to process one message at a time
        await rabbitMqChannel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false, cancellationToken: cancellationToken);

        var consumer = new AsyncEventingBasicConsumer(rabbitMqChannel);

        consumer.ReceivedAsync += async (_, ea) =>
        {
            var body = ea.Body;
            var message = JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(body.Span));

            if (message != null)
            {
                var tcs = new TaskCompletionSource<Result<T>>();
                await channel.Writer.WriteAsync((message, tcs), cancellationToken); 
                var result = await tcs.Task;

                if (result.IsSuccess)
                {
                    await rabbitMqChannel.BasicAckAsync(ea.DeliveryTag, false, cancellationToken);
                    _logger.LogInformation("Message from queue {QueueName}, processed successfully.", queueName);
                }
                else
                {
                    await rabbitMqChannel.BasicNackAsync(ea.DeliveryTag, false, false, cancellationToken);
                    _logger.LogError("Message from queue {QueueName}, failed with errors. Errors: {Errors}. {Message}", queueName, result.Errors, message);
                }
            }
        };

        await rabbitMqChannel.BasicConsumeAsync(queue: queueName, false, consumer: consumer, cancellationToken: cancellationToken);
        _logger.LogInformation("Started consuming messages from queue: {QueueName}", queueName);

        // Await channel completion or cancellation
        await channel.Reader.Completion.WaitAsync(cancellationToken);
    }

}