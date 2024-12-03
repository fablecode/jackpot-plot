namespace JackpotPlot.Domain.Messaging;

public interface IQueueWriter<in T>
{
    Task Publish(T message, string routingKey);
}