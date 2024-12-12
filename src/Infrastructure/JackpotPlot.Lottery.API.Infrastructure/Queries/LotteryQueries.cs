using System.Runtime.CompilerServices;
using JackpotPlot.Lottery.API.Infrastructure.Databases;
using Microsoft.EntityFrameworkCore;

namespace JackpotPlot.Lottery.API.Infrastructure.Queries;

public static class LotteryQueries
{
    public static readonly Func<LotteryDbContext, int, DateTime, int[], int[], bool> CheckDrawAndResultsCompiled =
        EF.CompileQuery((LotteryDbContext context, int lotteryId, DateTime drawDate, int[] winningNumbers, int[] bonusNumbers) =>
            context.Database
                .SqlQuery<bool>(FormattableStringFactory.Create(
                    "CALL CheckDrawAndResults({0}, {1}, {2}, {3})",
                    lotteryId,
                    drawDate,
                    winningNumbers,
                    bonusNumbers))
                .AsEnumerable()
                .FirstOrDefault());


    public static readonly Func<LotteryDbContext, int, DateTime, int[], int[], bool> CheckDrawAndResults = EF.CompileQuery(
        (LotteryDbContext context, int lotteryId, DateTime drawDate, int[] winningNumbers, int[] bonusNumbers) =>
            context.Draws
                .Any(d => d.LotteryId == lotteryId && d.DrawDate == DateOnly.FromDateTime(drawDate)) &&
            context.DrawResults
                .Join(context.Draws,
                    dr => dr.DrawId,
                    d => d.Id,
                    (dr, d) => new { DrawResult = dr, Draw = d })
                .Any(joined =>
                    joined.Draw.LotteryId == lotteryId &&
                    joined.Draw.DrawDate == DateOnly.FromDateTime(drawDate) &&
                    joined.DrawResult.Numbers.SequenceEqual(winningNumbers) &&
                    (joined.DrawResult.BonusNumbers == null || joined.DrawResult.BonusNumbers.SequenceEqual(bonusNumbers)))
    );
}