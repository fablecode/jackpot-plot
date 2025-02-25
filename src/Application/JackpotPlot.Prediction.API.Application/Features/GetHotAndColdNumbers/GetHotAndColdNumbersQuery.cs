using JackpotPlot.Domain.Models;
using JackpotPlot.Prediction.API.Application.Models.Output;
using MediatR;

namespace JackpotPlot.Prediction.API.Application.Features.GetHotAndColdNumbers;

public record GetHotAndColdNumbersQuery() : IRequest<Result<HotColdNumbersOutput>>;