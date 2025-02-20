using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Repositories;
using JackpotPlot.Prediction.API.Infrastructure.Databases;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Collections.Immutable;

namespace JackpotPlot.Prediction.API.Infrastructure.Repositories;

public class LotteryStatisticsRepository : ILotteryStatisticsRepository
{
    private readonly IDbContextFactory<PredictionDbContext> _factory;

    public LotteryStatisticsRepository(IDbContextFactory<PredictionDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<ImmutableArray<NumberStatus>> GetHotColdNumbers(int lotteryId, List<int> numbers, TimeSpan timeRange, string numberType)
    {
        if (numbers.Count == 0)
            return [];

        var intervalString = $"{(int)(timeRange.TotalDays / 30)} months";

        const string query = @"
            SELECT * FROM get_hot_cold_numbers(@LotteryId, @TimeRange::INTERVAL, @Numbers, @NumberType);";

        using (var context = await _factory.CreateDbContextAsync())
        {
            using (var connection = (NpgsqlConnection)context.Database.GetDbConnection())
            {
                await connection.OpenAsync();

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@LotteryId", lotteryId);
                    command.Parameters.AddWithValue("@Numbers", numbers);
                    command.Parameters.AddWithValue("@TimeRange", intervalString);
                    command.Parameters.AddWithValue("@NumberType", numberType);

                    using (var reader = command.ExecuteReader())
                    {
                        var numberStatuses = new List<NumberStatus>();

                        while (reader.Read())
                        {
                            numberStatuses.Add(new NumberStatus
                            (
                                reader.GetInt32(0),
                                reader.GetInt32(1),
                                reader.GetString(2),
                                numberType
                            ));
                        }

                        return [..numberStatuses];
                    }
                }

            }
        }
    }
}