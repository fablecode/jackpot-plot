using MediatR;

namespace LotteryDataCollector.Service.Application.BackgroundServices.Eurojackpot.FetchEurojackpotDrawHistory;

public record FetchEurojackpotDrawHistoryRequest : IRequest;