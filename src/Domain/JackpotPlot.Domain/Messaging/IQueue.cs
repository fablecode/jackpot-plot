namespace JackpotPlot.Domain.Messaging;

public interface IQueue<in T>
{
    Task Publish(T message, string routingKey);
}