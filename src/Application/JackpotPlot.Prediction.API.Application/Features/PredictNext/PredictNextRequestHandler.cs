using JackpotPlot.Domain.Interfaces;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Repositories;
using JackpotPlot.Prediction.API.Application.Models.Output;
using MediatR;

namespace JackpotPlot.Prediction.API.Application.Features.PredictNext;

public sealed class PredictNextRequestHandler : IRequestHandler<PredictNextRequest, Result<PredictNextResponse>>
{
    private readonly IEnumerable<IPredictionStrategy> _predictionStrategies;
    private readonly IPredictionRepository _predictionRepository;
    private readonly ILotteryStatisticsRepository _lotteryStatisticsRepository;

    public PredictNextRequestHandler(IEnumerable<IPredictionStrategy> predictionStrategies, IPredictionRepository predictionRepository, ILotteryStatisticsRepository lotteryStatisticsRepository)
    {
        _predictionStrategies = predictionStrategies;
        _predictionRepository = predictionRepository;
        _lotteryStatisticsRepository = lotteryStatisticsRepository;
    }
    public async Task<Result<PredictNextResponse>> Handle(PredictNextRequest request, CancellationToken cancellationToken)
    {
        var predictionStrategy = _predictionStrategies.Single(ps => ps.Handles(request.Strategy));

        var predictions = new List<PlayOutput>();

        for (var i = 0; i < request.NumberOfPlays; i++)
        {
            var predictionResult = await predictionStrategy.Predict(request.LotteryId);

            if (predictionResult.IsSuccess)
            {
                var newPrediction = await _predictionRepository.Add(request.UserId, predictionResult.Value);

                var timeRage = new DateTime(1900, 1, 1, 0, 0, 0).TimeOfDay;

                var classifiedMainNumbers = await _lotteryStatisticsRepository.GetHotColdNumbers(request.LotteryId, predictionResult.Value.PredictedNumbers.ToList(), timeRage, "main");
                var classifiedBonusNumbers = await _lotteryStatisticsRepository.GetHotColdNumbers(request.LotteryId, predictionResult.Value.BonusNumbers.ToList(), timeRage, "bonus");

                var prediction = new PlayOutput(i+1,
                    classifiedMainNumbers.AddRange(classifiedBonusNumbers)
                        .Select(n => new PredictionNumberOutput(n.Number, n.Frequency, n.Status, n.NumberType))
                        .ToArray());

                predictions.Add(prediction);
            }
            else
            {
                return Result<PredictNextResponse>.Failure(predictionResult.Errors);
            }
        }

        var response = new PredictNextResponse(request.LotteryId, request.NumberOfPlays, request.Strategy, [..predictions]);

        return Result<PredictNextResponse>.Success(response);
    }
}