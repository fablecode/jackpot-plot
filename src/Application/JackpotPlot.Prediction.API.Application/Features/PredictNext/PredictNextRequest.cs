using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using MediatR;

namespace JackpotPlot.Prediction.API.Application.Features.PredictNext;

public record PredictNextRequest(int LotteryId, int NumberOfPlays = 5, string Strategy = PredictionStrategyType.Random, int? UserId = null) : IRequest<Result<PredictNextResponse>>;