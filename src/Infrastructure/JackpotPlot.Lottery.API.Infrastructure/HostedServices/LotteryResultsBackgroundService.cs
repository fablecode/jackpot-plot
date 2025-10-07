using JackpotPlot.Domain.Messaging;
using MediatR;
using Microsoft.Extensions.Logging;

namespace JackpotPlot.Lottery.API.Infrastructure.HostedServices;

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