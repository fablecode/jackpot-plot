using Coravel.Invocable;
using JackpotPlot.Infrastructure.Jobs.Eurojackpot.FetchEurojackpotDrawHistory;
using MediatR;

namespace LotteryDataCollector.Service.Jobs.Eurojackpot;

public class FetchEurojackpotDrawHistoryJob : IInvocable
{
    private readonly ILogger<FetchEurojackpotDrawHistoryJob> _logger;
    private readonly IMediator _mediator;

    public FetchEurojackpotDrawHistoryJob(ILogger<FetchEurojackpotDrawHistoryJob> logger, IMediator mediator)

    {
        _logger = logger;
        _mediator = mediator;
    }

    public async Task Invoke()
    {
        _logger.LogInformation("Fetching EuroJackpot draw history...");
        await _mediator.Send(new FetchEurojackpotDrawHistoryRequest());
        _logger.LogInformation("Fetching EuroJackpot draw completed...");
    }
}