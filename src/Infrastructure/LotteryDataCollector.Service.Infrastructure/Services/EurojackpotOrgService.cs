using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Services;
using Newtonsoft.Json;
using System.Collections.Immutable;
using System.Data;

namespace LotteryDataCollector.Service.Infrastructure.Services;

public class EurojackpotOrgService : IEurojackpotService
{
    private readonly IHttpClientFactory _factory;

    public EurojackpotOrgService(IHttpClientFactory factory)
    {
        _factory = factory;
    }
    public IEnumerable<EurojackpotResult> GetAllDrawHistoryResults()
    {
        throw new NotImplementedException();
    }

    public async IAsyncEnumerable<EurojackpotResult> GetAllDrawHistoryResultsAsync()
    {
        foreach (var drawDate in GetEuroJackpotDrawDates())
        {
            var url = $"https://www.eurojackpot.org/results?date={drawDate:dd-MM-yyyy}";

            var lotteryData = await FetchDataAsync(url);

            yield return GetDetails(lotteryData);
        }
    }

    private static EurojackpotResult GetDetails(EurojackpotOrgResult eurojackpotOrgResult)
    {
        return new EurojackpotResult
        {
            Date = new DateTime
            (
                int.Parse(eurojackpotOrgResult.date.year), 
                int.Parse(eurojackpotOrgResult.date.month),
                int.Parse(eurojackpotOrgResult.date.day)
            ),
            MainNumbers = [..eurojackpotOrgResult.numbers.numbers],
            EuroNumbers = [..eurojackpotOrgResult.numbers.extraNumbers],
            JackpotAmount = eurojackpotOrgResult.jackpot.ToString(),
            PrizeBreakdown = ImmutableArray<DataTable>.Empty
        };
    }

    #region Private Helpers

    public static IEnumerable<DateTime> GetEuroJackpotDrawDates()
    {
        var startDate = new DateTime(2012, 3, 23); // First Eurojackpot draw (Friday)
        var secondDrawStartDate = new DateTime(2022, 3, 29); // Tuesday draws started around this time
        var today = DateTime.Today;

        for (var date = startDate; date <= today; date = date.AddDays(1))
        {
            if (date.DayOfWeek == DayOfWeek.Friday || (date >= secondDrawStartDate && date.DayOfWeek == DayOfWeek.Tuesday))
            {
                yield return date;
            }
        }
    }
    public async Task<EurojackpotOrgResult> FetchDataAsync(string url)
    {
        using (var client = _factory.CreateClient())
        {
            Console.WriteLine($"🔄 Sending request to: {url}");

            var response = await client.GetAsync(url);

            if (response.StatusCode == System.Net.HttpStatusCode.MovedPermanently ||
                response.StatusCode == System.Net.HttpStatusCode.Found)
            {
                // Capture redirect URL
                string? redirectUrl = response.Headers.Location?.ToString();

                if (!string.IsNullOrEmpty(redirectUrl))
                {
                    Console.WriteLine($"🔀 Redirected to: {redirectUrl}");
                    return await FetchDataAsync(redirectUrl); // Recursively follow the redirect
                }
                else
                {
                    throw new Exception("❌ No redirect URL found in headers.");
                }
            }

            var json = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<EurojackpotOrgResult>(json)!;
        }
    }

    #endregion
}


public class EurojackpotOrgResult
{
    public long nr { get; set; }
    public Date date { get; set; }
    public long jackpot { get; set; }
    public Numbers numbers { get; set; }
    public Odds odds { get; set; }
}

public class Date
{
    public string day { get; set; }
    public string dayOfWeek { get; set; }
    public string month { get; set; }
    public string year { get; set; }
}

public class Numbers
{
    public int[] numbers { get; set; }
    public int[] extraNumbers { get; set; }
}

public class Odds
{
    public Rank1 rank1 { get; set; }
    public Rank2 rank2 { get; set; }
    public Rank3 rank3 { get; set; }
    public Rank4 rank4 { get; set; }
    public Rank5 rank5 { get; set; }
    public Rank6 rank6 { get; set; }
    public Rank7 rank7 { get; set; }
    public Rank8 rank8 { get; set; }
    public Rank9 rank9 { get; set; }
    public Rank10 rank10 { get; set; }
    public Rank11 rank11 { get; set; }
    public Rank12 rank12 { get; set; }
}

public class Rank1
{
    public string rank { get; set; }
    public long? winners { get; set; }
    public long? prize { get; set; }
}

public class Rank2
{
    public string rank { get; set; }
    public long? winners { get; set; }
    public long? prize { get; set; }
}

public class Rank3
{
    public string rank { get; set; }
    public long? winners { get; set; }
    public long? prize { get; set; }
}

public class Rank4
{
    public string rank { get; set; }
    public long? winners { get; set; }
    public long? prize { get; set; }
}

public class Rank5
{
    public string rank { get; set; }
    public long? winners { get; set; }
    public long? prize { get; set; }
}

public class Rank6
{
    public string rank { get; set; }
    public long? winners { get; set; }
    public long? prize { get; set; }
}

public class Rank7
{
    public string rank { get; set; }
    public long? winners { get; set; }
    public long? prize { get; set; }
}

public class Rank8
{
    public string rank { get; set; }
    public long? winners { get; set; }
    public long? prize { get; set; }
}

public class Rank9
{
    public string rank { get; set; }
    public long winners { get; set; }
    public long prize { get; set; }
}

public class Rank10
{
    public string rank { get; set; }
    public long? winners { get; set; }
    public long? prize { get; set; }
}

public class Rank11
{
    public string rank { get; set; }
    public long? winners { get; set; }
    public long? prize { get; set; }
}

public class Rank12
{
    public string rank { get; set; }
    public long? winners { get; set; }
    public long? prize { get; set; }
}
