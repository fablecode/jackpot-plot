using JackpotPlot.Domain.Domain;

namespace JackpotPlot.Application.Abstractions.Persistence.Repositories;

public interface ILotteryConfigurationRepository
{
    Task<LotteryConfigurationDomain?> GetActiveConfigurationAsync(int lotteryId);
}