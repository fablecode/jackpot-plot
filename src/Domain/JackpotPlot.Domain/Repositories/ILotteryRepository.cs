using JackpotPlot.Domain.Domain;

namespace JackpotPlot.Domain.Repositories;

public interface ILotteryRepository
{
    Task<int> GetLotteryIdByName(string name);
    Task<ICollection<LotteryDomain>> GetLotteries();
}