using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Models;

namespace JackpotPlot.Domain.Messaging.Handlers;

public interface IMessageHandler<in T>
{
    bool Handles(string eventName);

    Task Process(T message, CancellationToken cancellationToken);
}

public sealed class EurojackpotResultHandler : IMessageHandler<EurojackpotResult>
{
    public bool Handles(string eventName) => eventName.Equals(EventTypes.EurojackpotDraw);
    public Task Process(EurojackpotResult message, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}