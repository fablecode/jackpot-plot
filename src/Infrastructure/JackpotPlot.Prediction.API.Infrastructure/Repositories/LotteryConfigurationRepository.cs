using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Repositories;
using JackpotPlot.Prediction.API.Infrastructure.Services.LotteryApi;

namespace JackpotPlot.Prediction.API.Infrastructure.Repositories;

public class LotteryConfigurationRepository : ILotteryConfigurationRepository
{
    private readonly ILotteryService _lotteryService;

    public LotteryConfigurationRepository(ILotteryService lotteryService)
    {
        _lotteryService = lotteryService;
    }
    public async Task<LotteryConfigurationDomain?> GetActiveConfigurationAsync(int lotteryId)
    {
        return await _lotteryService.GetConfiguration(lotteryId);
    }
}