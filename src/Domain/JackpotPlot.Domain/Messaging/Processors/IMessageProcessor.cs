using JackpotPlot.Domain.Models;

namespace JackpotPlot.Domain.Messaging.Processors;

public interface IMessageProcessor<T>
{
    Task<Result<T>> ProcessAsync(T message, CancellationToken cancellationToken);
}