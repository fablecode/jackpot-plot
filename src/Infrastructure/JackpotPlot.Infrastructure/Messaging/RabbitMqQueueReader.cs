using System.Text;
using System.Threading.Channels;
using JackpotPlot.Domain.Messaging;
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
    public async ValueTask Subscribe(string queueName, Channel<T> channel, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting subscription to queue: {QueueName}", queueName);

        await using var rabbitMqConnection = await _factory.CreateConnectionAsync(cancellationToken);
        await using var rabbitMqChannel = await rabbitMqConnection.CreateChannelAsync(cancellationToken: cancellationToken);

        var consumer = new AsyncEventingBasicConsumer(rabbitMqChannel);

        consumer.ReceivedAsync += async (_, ea) =>
        {
            try
            {
                var body = ea.Body;
                var message = JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(body.Span));

                if (message != null)
                {
                    await channel.Writer.WriteAsync(message, cancellationToken);
                    await rabbitMqChannel.BasicAckAsync(ea.DeliveryTag, false, cancellationToken);
                }
                else
                {
                    // Log or handle null message scenario
                    await rabbitMqChannel.BasicNackAsync(ea.DeliveryTag, false, false, cancellationToken); // Reject and discard
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message from queue: {QueueName}, DeliveryTag: {DeliveryTag}", queueName, ea.DeliveryTag);
                await rabbitMqChannel.BasicNackAsync(ea.DeliveryTag, false, false, cancellationToken);
                // Optionally log the exception
            }
        };

        await rabbitMqChannel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer, cancellationToken: cancellationToken);
        _logger.LogInformation("Started consuming messages from queue: {QueueName}", queueName);

        try
        {
            // Await channel completion or cancellation
            await channel.Reader.Completion.WaitAsync(cancellationToken);
        }
        catch (OperationCanceledException ex)
        {
            // Log or handle cancellation
            _logger.LogWarning(ex, "Subscription to queue: {QueueName} was cancelled.", queueName);

        }
        finally
        {
            // Ensure proper cleanup
            consumer.ReceivedAsync -= null;
            _logger.LogInformation("Cleaned up consumer event handlers for queue: {QueueName}.", queueName);
        }
    }

}