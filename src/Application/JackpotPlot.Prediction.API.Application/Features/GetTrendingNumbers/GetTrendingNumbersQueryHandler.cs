using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Repositories;
using MediatR;

namespace JackpotPlot.Prediction.API.Application.Features.GetTrendingNumbers;

public sealed class GetTrendingNumbersQueryHandler : IRequestHandler<GetTrendingNumbersQuery, Result<Dictionary<int, int>>>
{
    private readonly IPredictionRepository _predictionRepository;

    public GetTrendingNumbersQueryHandler(IPredictionRepository predictionRepository)
    {
        _predictionRepository = predictionRepository;
    }

    public async Task<Result<Dictionary<int, int>>> Handle(GetTrendingNumbersQuery request, CancellationToken cancellationToken)
    {
        var result = await _predictionRepository.GetTrendingNumbers();

        return Result<Dictionary<int, int>>.Success(result);
    }
}