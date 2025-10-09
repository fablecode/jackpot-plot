using JackpotPlot.Application.Abstractions.Common;

namespace JackpotPlot.Infrastructure.Common;

public sealed class DefaultRandomProvider : IRandomProvider
{
    public Random Get() => new();
}