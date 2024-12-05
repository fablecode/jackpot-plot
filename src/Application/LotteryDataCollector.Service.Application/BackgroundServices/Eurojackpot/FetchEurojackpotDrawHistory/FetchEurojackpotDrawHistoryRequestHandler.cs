using JackpotPlot.Domain.Domain;
using MediatR;

namespace LotteryDataCollector.Service.Application.BackgroundServices.Eurojackpot.FetchEurojackpotDrawHistory;

public sealed class FetchEurojackpotDrawHistoryRequestHandler : IRequestHandler<FetchEurojackpotDrawHistoryRequest>
{
    private readonly EurojackpotJobs _jobs;

    public FetchEurojackpotDrawHistoryRequestHandler(EurojackpotJobs jobs)
    {
        _jobs = jobs;
    }
    public async Task Handle(FetchEurojackpotDrawHistoryRequest request, CancellationToken cancellationToken)
    {
        await _jobs.FetchDrawHistory();
    }
}