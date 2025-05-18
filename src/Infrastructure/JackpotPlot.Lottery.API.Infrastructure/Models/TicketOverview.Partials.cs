using System.ComponentModel.DataAnnotations.Schema;
using JackpotPlot.Domain.Enums;
using Newtonsoft.Json;

namespace JackpotPlot.Lottery.API.Infrastructure.Models;

public partial class TicketOverview
{
    [Column(TypeName = "ticket_status")]
    [JsonProperty("status")]
    public TicketStatus Status { get; set; }

    [Column(TypeName = "confidence_level")]
    [JsonProperty("confidence")]
    public ConfidenceLevel Confidence { get; set; }
}