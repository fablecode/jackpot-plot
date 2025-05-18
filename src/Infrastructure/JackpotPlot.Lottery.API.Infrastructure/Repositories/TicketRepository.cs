using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Repositories;
using JackpotPlot.Lottery.API.Infrastructure.Databases;
using JackpotPlot.Lottery.API.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Collections.Immutable;
using System.Text.Json;
using JackpotPlot.Lottery.API.Infrastructure.Results;
using Newtonsoft.Json;

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
                LotteryId = ticket.LotteryId,
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

    public async Task<PagedTickets> SearchTickets(
        int pageNumber,
        int pageSize,
        Guid? userId,
        string? searchTerm = null,
        string sortColumn = "ticket_id",
        string sortDirection = "asc")
    {
        using (var context = await _contextFactory.CreateDbContextAsync())
        {
            var connectionString = context.Database.GetConnectionString();

            using (var connection = new NpgsqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var command = new NpgsqlCommand("SELECT get_paginated_ticket_overview_json(@page_number, @page_size, @user_id, @search_term, @sort_column, @sort_direction)", connection))
                {
                    command.Parameters.AddWithValue("page_number", pageNumber);
                    command.Parameters.AddWithValue("page_size", pageSize);
                    command.Parameters.AddWithValue("user_id", (object?)userId ?? DBNull.Value);
                    command.Parameters.AddWithValue("search_term", (object?)searchTerm ?? DBNull.Value);
                    command.Parameters.AddWithValue("sort_column", sortColumn);
                    command.Parameters.AddWithValue("sort_direction", sortDirection);

                    var json = (string)(await command.ExecuteScalarAsync())!;

                    var pagedTickets = JsonConvert.DeserializeObject<PaginatedTicketOverviewResult>(json)
                           ?? new PaginatedTicketOverviewResult();

                    return new PagedTickets
                    {
                        TotalItems = pagedTickets.TotalItems,
                        TotalFilteredItems = pagedTickets.TotalFilteredItems,
                        TotalPages = pagedTickets.TotalPages,
                        Tickets = pagedTickets.Tickets.Select(pt => new PagedTicketItem
                            {
                                TicketId = pt.TicketId,
                                TicketName = pt.TicketName,
                                LotteryName = pt.LotteryName,
                                Status = pt.Status,
                                Entries = pt.Entries,
                                Confidence = pt.Confidence
                            })
                            .ToList()
                    };
                }
            }
        }
    }
}