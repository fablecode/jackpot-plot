using JackpotPlot.Domain.Models;
using MediatR;

namespace JackpotPlot.Prediction.API.Application.Features.GetTrendingNumbers;

public record GetTrendingNumbersQuery : IRequest<Result<Dictionary<int, int>>>;