namespace JackpotPlot.Lottery.API.Application.Models.Output;

public record TicketOutput(Guid Id, string Name, bool IsPublic, int PlayCount);