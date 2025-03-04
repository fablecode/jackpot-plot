using JackpotPlot.Domain.Models;
using MediatR;

namespace JackpotPlot.Prediction.API.Application.Features.GetNumberSpread;

public record GetNumberSpreadQuery() : IRequest<Result<NumberSpreadResult>>;