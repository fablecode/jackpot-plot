using JackpotPlot.Domain.Models;
using JackpotPlot.Lottery.API.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Collections.Immutable;
using JackpotPlot.Application.Abstractions.Persistence.Repositories;
using JackpotPlot.Lottery.API.Infrastructure.Databases;

namespace JackpotPlot.Lottery.API.Infrastructure.Repositories;

public class DrawRepository : IDrawRepository
{
    private readonly IDbContextFactory<LotteryDbContext> _contextFactory;

    public DrawRepository(IDbContextFactory<LotteryDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<int> Add(int lotteryId, EurojackpotResult draw)
    {
        using (var context = await _contextFactory.CreateDbContextAsync())
        {
            var newDraw = new Draw
            {
                LotteryId = lotteryId,
                DrawDate = DateOnly.FromDateTime(draw.Date),
                JackpotAmount = draw.GetJackpotAmount(),
                RolloverCount = draw.Rollover
            };

            var addedDraw = await context.Draws.AddAsync(newDraw);

            await context.SaveChangesAsync();

            return addedDraw.Entity.Id;
        }
    }

    public async Task<bool> DrawExist(int lotteryId, DateTime drawDate, ImmutableArray<int> mainNumbers, ImmutableArray<int> bonusNumbers)
    {
        using (var context = await _contextFactory.CreateDbContextAsync())
        {
            // Create parameters
            var lotteryIdParameter = new NpgsqlParameter<int>("LotteryId", lotteryId);
            var drawDateParameter = new NpgsqlParameter<DateTime>("DrawDate", drawDate);
            var winningNumbersParameter = new NpgsqlParameter<int[]>("WinningNumbers", mainNumbers.ToArray());
            var bonusNumbersParameter = new NpgsqlParameter<int[]>("BonusNumbers", bonusNumbers.ToArray());
            var resultParameter = new NpgsqlParameter<bool>
            {
                ParameterName = "Result",
                DbType = System.Data.DbType.Boolean,
                Direction = System.Data.ParameterDirection.InputOutput
            };

            //// Execute stored procedure
            await context.Database.ExecuteSqlRawAsync(
                "CALL CheckDrawAndResults(@LotteryId, @DrawDate, @WinningNumbers, @BonusNumbers, @Result)",
                lotteryIdParameter, drawDateParameter, winningNumbersParameter, bonusNumbersParameter, resultParameter
            );

            //// Retrieve the OUT parameter value
            return (bool)resultParameter.Value!;
        }
    }
}