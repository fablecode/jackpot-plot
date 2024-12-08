using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Repositories;
using JackpotPlot.Lottery.API.Infrastructure.Databases;
using JackpotPlot.Lottery.API.Infrastructure.Models;
using JackpotPlot.Lottery.API.Infrastructure.Queries;
using Microsoft.EntityFrameworkCore;
using Npgsql;

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
                JackpotAmount = decimal.Parse(draw.JackpotAmount),
                RolloverCount = draw.Rollover
            };

            var addedDraw = await context.Draws.AddAsync(newDraw);

            return addedDraw.Entity.Id;
        }
    }

    public async Task<bool> DrawExist(int lotteryId, DateTime drawDate, ImmutableArray<int> mainNumbers, ImmutableArray<int> bonusNumbers)
    {
        using (var context = await _contextFactory.CreateDbContextAsync())
        {
            //var result = LotteryQueries.CheckDrawAndResultsCompiled(context, lotteryId, drawDate, mainNumbers.ToArray(), bonusNumbers.ToArray());
            //var result = context.Database
            //    .SqlQuery<bool>(FormattableStringFactory.Create(
            //        "CALL public.checkdrawandresults({0}, {1}, {2}, {3})",
            //        lotteryId,
            //        drawDate.ToUniversalTime(),
            //        mainNumbers,
            //        bonusNumbers))
            //    .AsEnumerable()
            //    .FirstOrDefault();

            // Create parameters
            var lotteryIdParameter = new NpgsqlParameter<int>("LotteryId", lotteryId);
            var drawDateParameter = new NpgsqlParameter<DateTime>("DrawDate", drawDate);
            var winningNumbersParameter = new NpgsqlParameter<int[]>("WinningNumbers", mainNumbers.ToArray());
            var bonusNumbersParameter = new NpgsqlParameter<int[]>("BonusNumbers", bonusNumbers.ToArray());
            var resultParameter = new NpgsqlParameter<bool>
            {
                ParameterName = "Result",
                DbType = System.Data.DbType.Boolean,
                Direction = System.Data.ParameterDirection.Output
            };

            //// Execute stored procedure
            await context.Database.ExecuteSqlRawAsync(
                "CALL public.checkdrawandresults(@LotteryId, @DrawDate, @WinningNumbers, @BonusNumbers)",
                lotteryIdParameter, drawDateParameter, winningNumbersParameter, bonusNumbersParameter, resultParameter
            );

            //// Retrieve the OUT parameter value
            return (bool)resultParameter.Value;
        }
    }
}