using System.Collections.Immutable;
using JackpotPlot.Domain.Models;
using MediatR;

namespace JackpotPlot.Prediction.API.Application.Features.GetLuckyPair;

public record GetLuckyPairQuery() : IRequest<Result<ImmutableArray<LuckyPairResult>>>;