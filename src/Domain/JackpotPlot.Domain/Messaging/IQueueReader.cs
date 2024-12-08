using JackpotPlot.Domain.Models;
using System.Threading.Channels;

namespace JackpotPlot.Domain.Messaging;

public interface IQueueReader<T>
{
    ValueTask Subscribe(string queueName, Channel<(T message, TaskCompletionSource<Result<T>> tcs)> channel, CancellationToken cancellationToken);
}