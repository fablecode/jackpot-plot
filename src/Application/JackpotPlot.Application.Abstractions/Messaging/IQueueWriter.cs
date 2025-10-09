namespace JackpotPlot.Application.Abstractions.Messaging;

public interface IQueueWriter<in T>
{
    Task Publish(T message, string routingKey);
}