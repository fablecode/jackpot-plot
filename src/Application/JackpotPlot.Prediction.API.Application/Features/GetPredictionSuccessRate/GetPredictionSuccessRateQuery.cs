using JackpotPlot.Domain.Models;
using MediatR;
using System.Collections.Immutable;

namespace JackpotPlot.Prediction.API.Application.Features.GetPredictionSuccessRate;

public record GetPredictionSuccessRateQuery : IRequest<Result<ImmutableDictionary<int, int>>>;