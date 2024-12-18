using JackpotPlot.Domain.Domain;
using Refit;

namespace JackpotPlot.Prediction.API.Infrastructure.Services.LotteryApi;

public interface ILotteryService
{
    [Get(LotteryServiceEndpoint.LotteryConfigurationById)]
    Task<LotteryConfigurationDomain> GetConfiguration(int id);

    [Get(LotteryServiceEndpoint.LotteryConfigurationsById)]
    Task<IEnumerable<LotteryConfigurationDomain>> GetConfigurations(int id);
}