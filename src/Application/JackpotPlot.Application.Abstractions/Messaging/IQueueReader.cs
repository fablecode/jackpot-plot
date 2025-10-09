using System.Threading.Channels;
using JackpotPlot.Domain.Models;

namespace JackpotPlot.Application.Abstractions.Messaging;

public interface IQueueReader<T>
{
    ValueTask Subscribe(string queueName, Channel<(T message, TaskCompletionSource<Result<T>> tcs)> channel, CancellationToken cancellationToken);
    ValueTask Subscribe(string queueName, HashSet<string> routingKeys, Channel<(T message, TaskCompletionSource<Result<T>> tcs)> channel, CancellationToken cancellationToken);
}