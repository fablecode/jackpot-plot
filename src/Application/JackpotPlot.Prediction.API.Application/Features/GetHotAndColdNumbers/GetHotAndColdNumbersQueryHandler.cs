using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Repositories;
using JackpotPlot.Prediction.API.Application.Models.Output;
using MediatR;

namespace JackpotPlot.Prediction.API.Application.Features.GetHotAndColdNumbers;

public sealed class GetHotAndColdNumbersQueryHandler : IRequestHandler<GetHotAndColdNumbersQuery, Result<HotColdNumbersOutput>>
{
    private readonly ILotteryHistoryRepository _lotteryHistoryRepository;

    public GetHotAndColdNumbersQueryHandler(ILotteryHistoryRepository lotteryHistoryRepository)
    {
        _lotteryHistoryRepository = lotteryHistoryRepository;
    }
    public async Task<Result<HotColdNumbersOutput>> Handle(GetHotAndColdNumbersQuery request, CancellationToken cancellationToken)
    {
        var result = await _lotteryHistoryRepository.GetAll();

        return Result<HotColdNumbersOutput>.Success(new HotColdNumbersOutput(result.hotNumbers, result.coldNumbers));
    }
}