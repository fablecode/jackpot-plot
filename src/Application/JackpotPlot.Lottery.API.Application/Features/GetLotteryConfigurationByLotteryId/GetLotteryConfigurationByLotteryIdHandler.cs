using JackpotPlot.Application.Abstractions.Persistence.Repositories;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using MediatR;

namespace JackpotPlot.Lottery.API.Application.Features.GetLotteryConfigurationByLotteryId;

public sealed class GetLotteryConfigurationByLotteryIdHandler : IRequestHandler<GetLotteryConfigurationByLotteryIdQuery, Result<LotteryConfigurationDomain>>
{
    private readonly ILotteryConfigurationRepository _lotteryConfigurationRepository;

    public GetLotteryConfigurationByLotteryIdHandler(ILotteryConfigurationRepository lotteryConfigurationRepository)
    {
        _lotteryConfigurationRepository = lotteryConfigurationRepository;
    }

    public async Task<Result<LotteryConfigurationDomain>> Handle(GetLotteryConfigurationByLotteryIdQuery request, CancellationToken cancellationToken)
    {
        var result = await _lotteryConfigurationRepository.GetActiveConfigurationAsync(request.LotteryId);

        if (result != null)
        {
            return Result<LotteryConfigurationDomain>.Success(result);
        }

        return Result<LotteryConfigurationDomain>.Failure($"Lottery configuration with id {request.LotteryId}, not found.");
    }
}