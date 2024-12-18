namespace JackpotPlot.Prediction.API.Infrastructure.Services.LotteryApi;

public static class LotteryServiceEndpoint
{
    private const string Api = "/api";
    public const string LotteryConfigurationById = Api + "/lotteries/{Id}/configuration";
    public const string LotteryConfigurationsById = Api + "/lotteries/{Id}/configurations";
}