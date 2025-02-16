using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using MediatR;

namespace JackpotPlot.Lottery.API.Application.Features.GetLotteries;

public record GetLotteriesQuery: IRequest<Result<ICollection<LotteryDomain>>>;