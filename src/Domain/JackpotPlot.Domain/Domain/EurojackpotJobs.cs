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
    private readonly IQueueWriter<Message<EurojackpotResult>> _queueWriter;

    public EurojackpotJobs(ILogger<EurojackpotJobs> logger, IEurojackpotService eurojackpotService, IQueueWriter<Message<EurojackpotResult>> queueWriter)
    {
        _logger = logger;
        _eurojackpotService = eurojackpotService;
        _queueWriter = queueWriter;
    }

    public async Task FetchDrawHistory()
    {
        await foreach(var result in _eurojackpotService.GetAllDrawHistoryResultsAsync())
        {
            var message = new Message<EurojackpotResult>(EventTypes.EurojackpotDraw, result);

            var routingKey = string.Join('.', RoutingKeys.LotteryResults, EventTypes.EurojackpotDraw);

            _logger.LogInformation("Publishing Eurojackpot draw result for {date}", result.Date);
            await _queueWriter.Publish(message, routingKey);
        }
    }
}