using JackpotPlot.Domain.Domain;

namespace JackpotPlot.Domain.Repositories;

public interface ILotteryConfigurationRepository
{
    Task<LotteryConfigurationDomain?> GetActiveConfigurationAsync(int lotteryId);
}