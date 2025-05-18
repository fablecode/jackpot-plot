using JackpotPlot.Lottery.API.Infrastructure.Models;
using Newtonsoft.Json;

namespace JackpotPlot.Lottery.API.Infrastructure.Results;

public class PaginatedTicketOverviewResult
{
    [JsonProperty("total_items")]
    public int TotalItems { get; set; }

    [JsonProperty("total_filtered_items")]
    public int TotalFilteredItems { get; set; }

    [JsonProperty("total_pages")]
    public int TotalPages { get; set; }
    public List<TicketOverview> Tickets { get; set; } = new();
}