using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Models;
using MediatR;

namespace JackpotPlot.Prediction.API.Application.Features.PredictNext;

public record PredictNextRequest(int LotteryId, int? UserId = null, string Strategy = PredictionStrategyType.Random) : IRequest<Result<int>>;