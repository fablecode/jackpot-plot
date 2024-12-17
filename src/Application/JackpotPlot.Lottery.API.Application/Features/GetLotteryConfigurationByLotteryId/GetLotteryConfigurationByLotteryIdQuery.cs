using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using MediatR;

namespace JackpotPlot.Lottery.API.Application.Features.GetLotteryConfigurationByLotteryId;

public record GetLotteryConfigurationByLotteryIdQuery(int LotteryId) : IRequest<Result<LotteryConfigurationDomain>>;