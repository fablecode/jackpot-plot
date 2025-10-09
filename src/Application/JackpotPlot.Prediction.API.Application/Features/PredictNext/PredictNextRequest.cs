using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Models;
using JackpotPlot.Primitives.Algorithms;
using MediatR;

namespace JackpotPlot.Prediction.API.Application.Features.PredictNext;

public record PredictNextRequest(int LotteryId, int NumberOfPlays = 5, string Strategy = PredictionAlgorithmKeys.Random, Guid? UserId = null) : IRequest<Result<PredictNextResponse>>;