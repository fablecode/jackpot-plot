using JackpotPlot.Domain.Domain;

namespace JackpotPlot.Application.Abstractions.Persistence.Repositories;

public interface ILotteryRepository
{
    Task<int> GetLotteryIdByName(string name);
    Task<ICollection<LotteryDomain>> GetLotteries();
}