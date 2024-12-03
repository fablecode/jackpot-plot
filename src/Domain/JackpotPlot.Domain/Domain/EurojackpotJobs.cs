using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Messaging;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Services;
using Microsoft.Extensions.Logging;

namespace JackpotPlot.Domain.Domain;

public sealed class EurojackpotJobs
{
    private readonly ILogger<EurojackpotJobs> _logger;
    private readonly IEurojackpotService _eurojackpotService;
    private readonly IQueue<Message<EurojackpotResult>> _queue;

    public EurojackpotJobs(ILogger<EurojackpotJobs> logger, IEurojackpotService eurojackpotService, IQueue<Message<EurojackpotResult>> queue)
    {
        _logger = logger;
        _eurojackpotService = eurojackpotService;
        _queue = queue;
    }

    public async Task FetchDrawHistory()
    {
        foreach(var result in _eurojackpotService.GetAllDrawHistoryResults())
        {
            var message = new Message<EurojackpotResult>(EventTypes.EurojackpotDraw, result);

            var routingKey = string.Join('.', RoutingKeys.LotteryDbUpdate, EventTypes.EurojackpotDraw);

            _logger.LogInformation("Publishing Eurojackpot draw result for {date}", result.Date);
            await _queue.Publish(message, routingKey);
        }
    }
}