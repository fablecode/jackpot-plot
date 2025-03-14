using System.Collections.Immutable;
using System.Threading;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Repositories;
using JackpotPlot.Prediction.API.Infrastructure.Databases;
using JackpotPlot.Prediction.API.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace JackpotPlot.Prediction.API.Infrastructure.Repositories;

public sealed class LotteryHistoryRepository : ILotteryHistoryRepository
{
    private readonly IDbContextFactory<PredictionDbContext> _factory;

    public LotteryHistoryRepository(IDbContextFactory<PredictionDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<(Dictionary<int, int> hotNumbers, Dictionary<int, int> coldNumbers)> GetAll()
    {
        using (var context = await _factory.CreateDbContextAsync())
        {
            var results = await context.Lotteryhistories.ToListAsync();

            // Count occurrences of all numbers
            var numberCounts = results.SelectMany(l => l.Winningnumbers)
                .GroupBy(n => n)
                .ToDictionary(g => g.Key, g => g.Count());

            // Find the 5 most common (hot) and 5 least common (cold) numbers
            var hotNumbers = numberCounts.OrderByDescending(x => x.Value).Take(5).ToDictionary(x => x.Key, x => x.Value);
            var coldNumbers = numberCounts.OrderBy(x => x.Value).Take(5).ToDictionary(x => x.Key, x => x.Value);

            return (hotNumbers, coldNumbers);
        }
    }

    public async Task<int> Add(LotteryDrawnEvent lotteryDrawnEvent)
    {
        using (var context = await _factory.CreateDbContextAsync())
        {
            var lotteryHistory = new Lotteryhistory
            {
                Lotteryid = lotteryDrawnEvent.LotteryId,
                Drawdate = lotteryDrawnEvent.DrawDate,
                Winningnumbers = lotteryDrawnEvent.WinningNumbers.ToList(),
                Bonusnumbers = lotteryDrawnEvent.BonusNumbers.ToList(),
            };

            var addedLotteryHistory = await context.Lotteryhistories.AddAsync(lotteryHistory);

            await context.SaveChangesAsync();

            return addedLotteryHistory.Entity.Id;
        }
    }

    public async Task<ICollection<HistoricalDraw>> GetHistoricalDraws(int lotteryId)
    {
        using (var context = await _factory.CreateDbContextAsync())
        {
            return await context.Lotteryhistories
                .Where(draw => draw.Lotteryid == lotteryId)
                .OrderByDescending(draw => draw.Drawdate) // Sort by most recent first
                .Select(lh => new HistoricalDraw(lh.Id, lh.Lotteryid, lh.Drawdate, lh.Winningnumbers, lh.Bonusnumbers!, lh.Createdat!.Value))
                .ToListAsync();
        }
    }

    public async Task<ImmutableArray<WinningNumberFrequencyResult>> GetWinningNumberFrequency()
    {
        using (var context = await _factory.CreateDbContextAsync())
        {
            var results = new List<WinningNumberFrequencyResult>();
            var frequencyDict = new Dictionary<int, Dictionary<string, int>>();

            using (var conn = (NpgsqlConnection)context.Database.GetDbConnection()) // ✅ Get PostgreSQL connection
            {
                await conn.OpenAsync();

                using (var cmd = new NpgsqlCommand("SELECT * FROM get_winning_number_frequency_over_time()", conn))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            int number = reader.GetInt32(0);
                            string date = reader.GetDateTime(1).ToString("yyyy-MM-dd");
                            int frequency = reader.GetInt32(2);

                            if (!frequencyDict.ContainsKey(number))
                                frequencyDict[number] = new Dictionary<string, int>();

                            frequencyDict[number][date] = frequency;
                        }
                    }
                }
            }

            return [..frequencyDict.Select(kv => new WinningNumberFrequencyResult { Number = kv.Key, FrequencyOverTime = kv.Value })];
        }
    }
}