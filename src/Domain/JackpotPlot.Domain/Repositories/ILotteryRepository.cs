namespace JackpotPlot.Domain.Repositories;

public interface ILotteryRepository
{
    Task<int> GetLotteryIdByName(string name);
}