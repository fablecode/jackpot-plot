using JackpotPlot.Domain.Messaging;
using MediatR;

namespace Lottery.API.HostedServices;

public class LotteryResultsBackgroundService<T> : RabbitMqBackgroundService<T>
{

    public LotteryResultsBackgroundService
    (
        ILogger<LotteryResultsBackgroundService<T>> logger, 
        IQueueReader<T> queueReader,
        IMediator mediator) 
        : base(logger, mediator, queueReader)
    {
    }

    protected override string QueueName => "lottery-results";
}