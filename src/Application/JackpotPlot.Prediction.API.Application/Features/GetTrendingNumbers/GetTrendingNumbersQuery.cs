using JackpotPlot.Domain.Models;
using JackpotPlot.Prediction.API.Application.Models.Output;
using MediatR;

namespace JackpotPlot.Prediction.API.Application.Features.GetTrendingNumbers;

public record GetTrendingNumbersQuery : IRequest<Result<Dictionary<int, int>>>;