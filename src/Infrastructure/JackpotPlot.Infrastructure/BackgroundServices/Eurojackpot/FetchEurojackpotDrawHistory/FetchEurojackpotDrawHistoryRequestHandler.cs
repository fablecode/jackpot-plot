using JackpotPlot.Infrastructure.Jobs;
using MediatR;

namespace JackpotPlot.Infrastructure.BackgroundServices.Eurojackpot.FetchEurojackpotDrawHistory;

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