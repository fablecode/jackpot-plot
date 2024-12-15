using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Messaging;
using MediatR;

namespace Prediction.API.HostedServices;

public class LotteryDrawnBackgroundService<T> : RabbitMqBackgroundService<T>
{

    public LotteryDrawnBackgroundService
    (
        ILogger<LotteryDrawnBackgroundService<T>> logger, 
        IQueueReader<T> queueReader,
        IMediator mediator) 
        : base(logger, mediator, queueReader)
    {
    }

    protected override string QueueName => "lottery-db-update";
    protected override HashSet<string> RoutingKeys =>
    [
        string.Join('.', JackpotPlot.Domain.Constants.RoutingKeys.LotteryDbUpdate, EventTypes.LotteryDrawn)
    ];
}