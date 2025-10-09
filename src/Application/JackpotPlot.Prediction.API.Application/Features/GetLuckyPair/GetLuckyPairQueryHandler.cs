using System.Collections.Immutable;
using JackpotPlot.Application.Abstractions.Persistence.Repositories;
using JackpotPlot.Domain.Models;
using MediatR;

namespace JackpotPlot.Prediction.API.Application.Features.GetLuckyPair;

public sealed class GetLuckyPairQueryHandler : IRequestHandler<GetLuckyPairQuery, Result<ImmutableArray<LuckyPairResult>>>
{
    private readonly IPredictionRepository _predictionRepository;

    public GetLuckyPairQueryHandler(IPredictionRepository predictionRepository)
    {
        _predictionRepository = predictionRepository;
    }

    public async Task<Result<ImmutableArray<LuckyPairResult>>> Handle(GetLuckyPairQuery request, CancellationToken cancellationToken)
    {
        var result = await _predictionRepository.GetLuckyPair();

        return Result<ImmutableArray<LuckyPairResult>>.Success(result);
    }
}