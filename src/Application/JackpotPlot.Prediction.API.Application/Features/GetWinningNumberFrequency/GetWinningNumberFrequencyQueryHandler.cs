using System.Collections.Immutable;
using JackpotPlot.Application.Abstractions.Persistence.Repositories;
using JackpotPlot.Domain.Models;
using MediatR;

namespace JackpotPlot.Prediction.API.Application.Features.GetWinningNumberFrequency;

public sealed class GetWinningNumberFrequencyQueryHandler : IRequestHandler<GetWinningNumberFrequencyQuery, Result<ImmutableArray<WinningNumberFrequencyResult>>>
{
    private readonly ILotteryHistoryRepository _lotteryHistoryRepository;

    public GetWinningNumberFrequencyQueryHandler(ILotteryHistoryRepository lotteryHistoryRepository)
    {
        _lotteryHistoryRepository = lotteryHistoryRepository;
    }

    public async Task<Result<ImmutableArray<WinningNumberFrequencyResult>>> Handle(GetWinningNumberFrequencyQuery request, CancellationToken cancellationToken)
    {
        var result = await _lotteryHistoryRepository.GetWinningNumberFrequency();

        return Result<ImmutableArray<WinningNumberFrequencyResult>>.Success(result);
    }
}