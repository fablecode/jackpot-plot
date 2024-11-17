using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Messaging;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Services;

namespace JackpotPlot.Domain.Domain;

public sealed class EurojackpotJobs
{
    private readonly IEurojackpotService _eurojackpotService;
    private readonly IQueue<Message<EurojackpotResult>> _queue;

    public EurojackpotJobs(IEurojackpotService eurojackpotService, IQueue<Message<EurojackpotResult>> queue)
    {
        _eurojackpotService = eurojackpotService;
        _queue = queue;
    }

    public async Task FetchDrawHistory()
    {
        foreach(var result in _eurojackpotService.GetAllDrawHistoryResults())
        {
            var message = new Message<EurojackpotResult>(EventTypes.UpdateDrawHistory, result);

            var routingKey = string.Join('.', RoutingKeys.LotteryDbUpdate, EventTypes.UpdateDrawHistory);

            await _queue.Publish(message, routingKey);
        }
    }
}