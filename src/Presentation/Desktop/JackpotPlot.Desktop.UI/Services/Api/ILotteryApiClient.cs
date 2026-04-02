using JackpotPlot.Desktop.UI.Services.Api.Models;
using Refit;

namespace JackpotPlot.Desktop.UI.Services.Api;

/// <summary>
/// Refit API client for Lottery service endpoints
/// </summary>
public interface ILotteryApiClient
{
    /// <summary>
    /// Get all available lotteries
    /// </summary>
    [Get("/api/lotteries")]
    Task<ApiResponse<List<LotteryDto>>> GetAllLotteriesAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get lottery configuration by ID
    /// </summary>
    [Get("/api/lotteries/{id}/configuration")]
    Task<ApiResponse<LotteryConfigurationDto>> GetLotteryConfigurationAsync(
        int id,
        CancellationToken cancellationToken = default);
}
