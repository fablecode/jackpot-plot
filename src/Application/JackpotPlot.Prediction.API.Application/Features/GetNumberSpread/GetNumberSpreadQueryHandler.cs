using JackpotPlot.Application.Abstractions.Persistence.Repositories;
using JackpotPlot.Domain.Models;
using MediatR;

namespace JackpotPlot.Prediction.API.Application.Features.GetNumberSpread;

public sealed class GetNumberSpreadQueryHandler : IRequestHandler<GetNumberSpreadQuery, Result<NumberSpreadResult>>
{
    private readonly IPredictionRepository _predictionRepository;

    public GetNumberSpreadQueryHandler(IPredictionRepository predictionRepository)
    {
        _predictionRepository = predictionRepository;
    }
    public async Task<Result<NumberSpreadResult>> Handle(GetNumberSpreadQuery request, CancellationToken cancellationToken)
    {
        var result = await _predictionRepository.GetNumberSpread();

        return Result<NumberSpreadResult>.Success(result);
    }
}