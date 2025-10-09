namespace JackpotPlot.Application.Abstractions.Messaging;

public record Message<T>(string Event, T Data);