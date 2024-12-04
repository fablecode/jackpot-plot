using System.Threading.Channels;

namespace JackpotPlot.Domain.Messaging;

public interface IQueueReader<T>
{
    ValueTask Subscribe(string queueName, Channel<T> channel, CancellationToken cancellationToken);
}