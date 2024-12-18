using JackpotPlot.Domain.Interfaces;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Repositories;
using MediatR;

namespace JackpotPlot.Prediction.API.Application.Features.PredictNext;

public sealed class PredictNextRequestHandler : IRequestHandler<PredictNextRequest, Result<int>>
{
    private readonly IEnumerable<IPredictionStrategy> _predictionStrategies;
    private readonly IPredictionRepository _predictionRepository;

    public PredictNextRequestHandler(IEnumerable<IPredictionStrategy> predictionStrategies, IPredictionRepository predictionRepository)
    {
        _predictionStrategies = predictionStrategies;
        _predictionRepository = predictionRepository;
    }
    public async Task<Result<int>> Handle(PredictNextRequest request, CancellationToken cancellationToken)
    {
        var predictionStrategy = _predictionStrategies.Single(ps => ps.Handles(request.Strategy));

        var predictionResult = await predictionStrategy.Predict(request.LotteryId);

        if (predictionResult.IsSuccess)
        {
            var predictionId = await _predictionRepository.Add(predictionResult.Value);
            return Result<int>.Success(predictionId);
        }

        return Result<int>.Failure(predictionResult.Errors);
    }
}