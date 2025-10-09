using JackpotPlot.Domain.Settings;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;
using JackpotPlot.Application.Abstractions.Messaging;

namespace JackpotPlot.Infrastructure.Messaging;

public class RabbitMqQueueWriter<T>: IQueueWriter<T>
{
    private readonly IOptions<RabbitMqSettings> _rabbitMqConfig;
    private readonly ConnectionFactory _factory;

    public RabbitMqQueueWriter(IOptions<RabbitMqSettings> rabbitMqConfig)
    {
        _rabbitMqConfig = rabbitMqConfig;
        _factory = new ConnectionFactory()
        {
            HostName = rabbitMqConfig.Value.Host, 
            Port = rabbitMqConfig.Value.Port, 
            UserName = rabbitMqConfig.Value.Username, 
            Password = rabbitMqConfig.Value.Password
        };
    }
    public async Task Publish(T message, string routingKey)
    {
        var jsonMessage = JsonConvert.SerializeObject(message, Formatting.Indented);
        var body = new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(jsonMessage));

        using (var connection = await _factory.CreateConnectionAsync())
        {
            using (var channel = await connection.CreateChannelAsync())
            {
                await channel.BasicPublishAsync(_rabbitMqConfig.Value.Exchange, routingKey, true, body);
            }
        }
    }
}