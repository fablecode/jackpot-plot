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
                    "SELECT CheckDrawAndResults({0}, {1}, {2}, {3})",
                    lotteryId,
                    drawDate,
                    winningNumbers,
                    bonusNumbers))
                .AsEnumerable()
                .FirstOrDefault());
}