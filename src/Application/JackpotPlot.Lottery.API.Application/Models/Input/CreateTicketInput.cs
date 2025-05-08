namespace JackpotPlot.Lottery.API.Application.Models.Input;

public record CreateTicketInput
{
public string Name { get; init; }
public int LotteryId { get; init; }
public CreateTicketPlaysInput[] Plays { get; init; } = [];

public CreateTicketInput(string name, CreateTicketPlaysInput[]? plays = null)
{
    Name = name;
    Plays = plays ?? [];
}
}