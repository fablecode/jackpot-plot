using JackpotPlot.Domain.Domain;
using JackpotPlot.Lottery.API.Infrastructure.Databases;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;
using JackpotPlot.Domain.Repositories;
using JackpotPlot.Lottery.API.Infrastructure.Models;

namespace JackpotPlot.Lottery.API.Infrastructure.Repositories;

public class TicketRepository : ITicketRepository
{
    private readonly IDbContextFactory<LotteryDbContext> _contextFactory;

    public TicketRepository(IDbContextFactory<LotteryDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<ImmutableArray<TicketDomain>> GetAllUserTickets(Guid userId)
    {
        using (var context = await _contextFactory.CreateDbContextAsync())
        {
            return context.Tickets
                    .Include(ut => ut.TicketPlays)
                    .Where(ticket => ticket.UserId == userId)
                    .Select(ticket => new TicketDomain
                    {
                        Id = ticket.Id,
                        UserId = ticket.UserId,
                        Name = ticket.Name,
                        Description = ticket.Description,
                        IsPublic = ticket.IsPublic,
                        UserTicketPlays = ticket.TicketPlays
                            .Select(play => new TicketPlayDomain
                            {
                                Id = play.Id,
                                TicketId = play.TicketId,
                                LineIndex = play.LineIndex,
                                Numbers = play.Numbers,
                                CreatedAt = play.CreatedAt
                            })
                            .ToList()
                    })
                    .ToImmutableArray()
;
        }
    }
    public async Task<TicketDomain?> GetTicketById(Guid id)
    {
        using (var context = await _contextFactory.CreateDbContextAsync())
        {
            return await context.Tickets
                .Include(ut => ut.TicketPlays)
                .Where(ticket => ticket.Id == id)
                .Select(ticket => new TicketDomain
                {
                    Id = ticket.Id,
                    UserId = ticket.UserId,
                    Name = ticket.Name,
                    Description = ticket.Description,
                    IsPublic = ticket.IsPublic,
                    UserTicketPlays = ticket.TicketPlays
                        .Select(play => new TicketPlayDomain
                        {
                            Id = play.Id,
                            TicketId = play.TicketId,
                            LineIndex = play.LineIndex,
                            Numbers = play.Numbers,
                            CreatedAt = play.CreatedAt
                        })
                        .ToList()
                })
                .SingleOrDefaultAsync();
        }
    }

    public async Task<Guid> Add(TicketDomain ticket)
    {
        using (var context = await _contextFactory.CreateDbContextAsync())
        {
            var newTicket = new Ticket
            {
                UserId = ticket.UserId,
                Name = ticket.Name,
                Description = ticket.Description,
                IsPublic = ticket.IsPublic,
                IsDeleted = ticket.IsDeleted,
                CreatedAt = DateTime.UtcNow,
                TicketPlays = ticket.UserTicketPlays.Select(tp => new TicketPlay
                {
                    LineIndex = tp.LineIndex,
                    Numbers = tp.Numbers,
                    CreatedAt = DateTime.UtcNow
                })
                .ToList()
            };

            await context.Tickets.AddAsync(newTicket);

            await context.SaveChangesAsync();

            return newTicket.Id;
        }
    }
}