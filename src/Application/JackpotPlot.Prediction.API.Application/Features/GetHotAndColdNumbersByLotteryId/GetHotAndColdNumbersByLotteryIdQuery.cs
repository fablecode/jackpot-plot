using JackpotPlot.Domain.Models;
using JackpotPlot.Prediction.API.Application.Models.Output;
using MediatR;

namespace JackpotPlot.Prediction.API.Application.Features.GetHotAndColdNumbersByLotteryId;

public record GetHotAndColdNumbersByLotteryIdQuery(int LotteryId) : IRequest<Result<HotColdNumbersOutput>>;
