using System.Collections.Immutable;
using JackpotPlot.Application.Abstractions.Persistence.Repositories;
using JackpotPlot.Lottery.API.Infrastructure.Databases;
using JackpotPlot.Lottery.API.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace JackpotPlot.Lottery.API.Infrastructure.Repositories;

public sealed class TicketPlayRepository : ITicketPlayRepository
{
    private readonly IDbContextFactory<LotteryDbContext> _contextFactory;

    public TicketPlayRepository(IDbContextFactory<LotteryDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<ImmutableArray<Guid>> Add(Guid ticketId, ImmutableArray<(int LineIndex, List<int> Numbers)> ticketPlays)
    {
        var ticketPlayIds = new List<TicketPlay>();

        using (var context = await _contextFactory.CreateDbContextAsync())
        {
            foreach (var (lineIndex, numbers) in ticketPlays)
            {
                var newPlay = new TicketPlay
                {
                    TicketId = ticketId,
                    LineIndex = lineIndex,
                    Numbers = numbers,
                    CreatedAt = DateTime.UtcNow
                };

                context.TicketPlays.Add(newPlay);

                ticketPlayIds.Add(newPlay);
            }

            await context.SaveChangesAsync();

            return [..ticketPlayIds.Select(p => p.Id)];
        }
    }

    public async Task<bool> Delete(params Guid[] ids)
    {
        using (var context = await _contextFactory.CreateDbContextAsync())
        {
            var result = await context.TicketPlays
                .Where(t => ids.Contains(t.Id))
                .ExecuteDeleteAsync();

            return result > 0;
        }
    }
}