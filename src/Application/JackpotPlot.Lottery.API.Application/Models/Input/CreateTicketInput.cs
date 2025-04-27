namespace JackpotPlot.Lottery.API.Application.Models.Input;

public record CreateTicketInput(string Name, CreateTicketPlaysInput[] Plays);