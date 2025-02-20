using System.Collections.Immutable;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Prediction.API.Application.Models.Output;
using MediatR;

namespace JackpotPlot.Prediction.API.Application.Features.PredictNext;

public record PredictNextResponse(int LotteryId, int NumberOfPlays, string Strategy, ImmutableArray<PredictionOutput> Predictions) : IRequest<Result<PredictionDomain>>;