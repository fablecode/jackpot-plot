using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Repositories;
using MediatR;
using System.Collections.Immutable;

namespace JackpotPlot.Prediction.API.Application.Features.GetPredictionSuccessRate;

public sealed class GetPredictionSuccessRateQueryHandler : IRequestHandler<GetPredictionSuccessRateQuery, Result<ImmutableDictionary<int, int>>>
{
    private readonly IPredictionRepository _predictionRepository;

    public GetPredictionSuccessRateQueryHandler(IPredictionRepository predictionRepository)
    {
        _predictionRepository = predictionRepository;
    }
    public async Task<Result<ImmutableDictionary<int, int>>> Handle(GetPredictionSuccessRateQuery request, CancellationToken cancellationToken)
    {
        var result = await _predictionRepository.GetPredictionSuccessRate();

        return Result<ImmutableDictionary<int, int>>.Success(result);
    }
}