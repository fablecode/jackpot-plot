using JackpotPlot.Application.Abstractions.Persistence.Repositories;
using JackpotPlot.Domain.Models;
using JackpotPlot.Prediction.API.Application.Models.Output;
using MediatR;

namespace JackpotPlot.Prediction.API.Application.Features.GetHotAndColdNumbersByLotteryId;

public sealed class GetHotAndColdNumbersByLotteryIdQueryHandler : IRequestHandler<GetHotAndColdNumbersByLotteryIdQuery, Result<HotColdNumbersOutput>>
{
    private readonly IPredictionRepository _predictionRepository;

    public GetHotAndColdNumbersByLotteryIdQueryHandler(IPredictionRepository predictionRepository)
    {
        _predictionRepository = predictionRepository;
    }

    public async Task<Result<HotColdNumbersOutput>> Handle(GetHotAndColdNumbersByLotteryIdQuery request, CancellationToken cancellationToken)
    {
        var result = await _predictionRepository.GetHotColdNumbersByLotteryId(request.LotteryId);

        return Result<HotColdNumbersOutput>.Success(new HotColdNumbersOutput(result.hotNumbers, result.coldNumbers));

    }
}