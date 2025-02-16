using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Repositories;
using MediatR;

namespace JackpotPlot.Lottery.API.Application.Features.GetLotteries;

public sealed class GetLotteriesQueryHandler : IRequestHandler<GetLotteriesQuery, Result<ICollection<LotteryDomain>>>
{
    private readonly ILotteryRepository _lotteryRepository;

    public GetLotteriesQueryHandler(ILotteryRepository lotteryRepository)
    {
        _lotteryRepository = lotteryRepository;
    }

    public async Task<Result<ICollection<LotteryDomain>>> Handle(GetLotteriesQuery request, CancellationToken cancellationToken)
    {
        var result = await _lotteryRepository.GetLotteries();

        return Result<ICollection<LotteryDomain>>.Success(result);
    }
}