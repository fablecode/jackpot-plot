using JackpotPlot.Domain.Models;
using MediatR;
using System.Collections.Immutable;

namespace JackpotPlot.Prediction.API.Application.Features.GetWinningNumberFrequency;

public record GetWinningNumberFrequencyQuery() : IRequest<Result<ImmutableArray<WinningNumberFrequencyResult>>>;