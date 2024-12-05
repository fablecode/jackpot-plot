using JackpotPlot.Domain.Repositories;

namespace JackpotPlot.Lottery.API.Infrastructure.Repositories;

public sealed class LotteryRepository : ILotteryRepository
{
    public Task<int> GetLotteryIdByName(string name)
    {
        throw new NotImplementedException();
    }
}