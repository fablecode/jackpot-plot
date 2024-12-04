using System.Text;
using System.Threading.Channels;
using JackpotPlot.Domain.Messaging;
using JackpotPlot.Domain.Settings;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace JackpotPlot.Infrastructure.Messaging;

public class RabbitMqQueueReader<T> : IQueueReader<T>
{
    private readonly ConnectionFactory _factory;

    public RabbitMqQueueReader(IOptions<RabbitMqSettings> rabbitMqConfig)
    {
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
        using (var rabbitMqConnection = await _factory.CreateConnectionAsync(cancellationToken))
        {
            using (var rabbitMqChannel = await rabbitMqConnection.CreateChannelAsync(cancellationToken: cancellationToken))
            {
                var consumer = new AsyncEventingBasicConsumer(rabbitMqChannel);

                consumer.ReceivedAsync += async (_, ea) =>
                {
                    var body = ea.Body;
                    var message = JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(body.Span));

                    if (message != null)
                    {
                        // Write the message to the channel
                        await channel.Writer.WriteAsync(message, cancellationToken);
                    }

                    // Acknowledge the message
                    await rabbitMqChannel.BasicAckAsync(ea.DeliveryTag, false, cancellationToken);
                };

                await rabbitMqChannel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer, cancellationToken: cancellationToken);

                // Wait for the completion of the channel.Reader or cancellation
                await channel.Reader.Completion.WaitAsync(cancellationToken);
            }
        }
    }
}