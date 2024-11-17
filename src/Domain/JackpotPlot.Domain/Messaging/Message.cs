namespace JackpotPlot.Domain.Messaging;

public record Message<T>(string Event, T Data);