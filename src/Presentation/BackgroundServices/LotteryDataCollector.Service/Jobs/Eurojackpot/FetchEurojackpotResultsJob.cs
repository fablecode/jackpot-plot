using Coravel.Invocable;
using LotteryDataCollector.Service.Application.BackgroundServices.Eurojackpot.FetchEurojackpotDrawHistory;
using MediatR;
using NodaTime;

namespace LotteryDataCollector.Service.Jobs.Eurojackpot;

public class FetchEurojackpotResultsJob : IInvocable
{
    private readonly ILogger<FetchEurojackpotResultsJob> _logger;
    private readonly IMediator _mediator;

    public FetchEurojackpotResultsJob(ILogger<FetchEurojackpotResultsJob> logger, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }
    public async Task Invoke()
    {
        // Use NodaTime to get the current date and time in the Brussels time zone (CET/CEST)
        var systemClock = SystemClock.Instance;
        var now = systemClock.GetCurrentInstant();
        var zone = DateTimeZoneProviders.Tzdb["Europe/Brussels"];
        var localDateTime = now.InZone(zone).LocalDateTime;

        // Check if the current day is Tuesday or Friday, and time is 8:00 PM local time
        if ((localDateTime.DayOfWeek == IsoDayOfWeek.Tuesday || localDateTime.DayOfWeek == IsoDayOfWeek.Friday)
            && localDateTime.Hour == 20)
        {
            _logger.LogInformation("Fetching EuroJackpot latest draw...");
            await _mediator.Send(new FetchEurojackpotDrawHistoryRequest());
            _logger.LogInformation("Fetching EuroJackpot latest draw completed...");
        }
    }
}