using JackpotPlot.Application.Abstractions.Services;
using JackpotPlot.Domain.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Immutable;
using System.Data;
using System.Net;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace LotteryDataCollector.Service.Infrastructure.Services;

public class EurojackpotOrgService : IEurojackpotService
{
    private static readonly EventId EvtFetch = new(1001, nameof(FetchDataAsync));
    private static readonly EventId EvtIterate = new(1002, nameof(GetAllDrawHistoryResultsAsync));
    private static readonly EventId EvtMap = new(1003, "MapToDomain");

    private readonly ILogger<EurojackpotOrgService> _logger;
    private readonly IHttpClientFactory _factory;

    public EurojackpotOrgService(ILogger<EurojackpotOrgService> logger, IHttpClientFactory factory)
    {
        _logger = logger;
        _factory = factory;
    }

    public IEnumerable<EurojackpotResult> GetAllDrawHistoryResults() =>
        throw new NotImplementedException();

    public async IAsyncEnumerable<EurojackpotResult> GetAllDrawHistoryResultsAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(EvtIterate, "Starting draw history enumeration.");

        foreach (var drawDate in EurojackpotHelper.GetEuroJackpotDrawDates())
        {
            cancellationToken.ThrowIfCancellationRequested();

            using var scope = _logger.BeginScope(new Dictionary<string, object?>
            {
                ["DrawDate"] = drawDate
            });

            var url = $"https://www.eurojackpot.org/proxy.php?d=eurojackpot&t={DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
            _logger.LogDebug(EvtIterate, "Fetching draw data from {Url}.", url);

            EurojackpotProxyResponse? dto = null;
            try
            {
                dto = await FetchDataAsync(url, cancellationToken);
            }
            catch (JsonReaderException ex)
            {
                _logger.LogWarning(EvtIterate, ex,
                    "JSON reader error for {Url}. Skipping this date.", url);
                continue;
            }
            catch (JsonSerializationException ex)
            {
                _logger.LogWarning(EvtIterate, ex,
                    "JSON shape/serialization issue for {Url}. Skipping this date.", url);
                continue;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(EvtIterate, ex,
                    "HTTP error while fetching {Url}. Skipping this date.", url);
                continue;
            }
            catch (Exception ex)
            {
                _logger.LogError(EvtIterate, ex,
                    "Unexpected error while fetching {Url}. Skipping this date.", url);
                continue;
            }

            if (dto is null)
            {
                _logger.LogInformation(EvtIterate,
                    "No usable payload for {Url} (null/empty/array-of-nulls). Skipping.", url);
                continue;
            }

            EurojackpotResult? mapped = null;
            try
            {
                mapped = GetDetails(dto);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(EvtMap, ex,
                    "Failed to map DTO to domain for {Url}. Skipping.", url);
                continue;
            }

            if (mapped is null)
            {
                _logger.LogInformation(EvtMap,
                    "Mapped result is null for {Url}. Skipping.", url);
                continue;
            }

            _logger.LogDebug(EvtIterate,
                "Yielding result for draw {Date}.", mapped.Date);
            yield return mapped;
        }

        _logger.LogInformation(EvtIterate, "Completed draw history enumeration.");
    }

    private static EurojackpotResult? GetDetails(EurojackpotProxyResponse dto)
    {
        var draw = dto.last;
        if (draw?.date is null)
            return null;

        return new EurojackpotResult
        {
            Date = new DateTime(draw.date.year, draw.date.month, draw.date.day),
            MainNumbers = [.. (draw.numbers ?? [])],
            EuroNumbers = [.. (draw.euroNumbers ?? [])],
            JackpotAmount = draw.jackpot ?? string.Empty,
            PrizeBreakdown = ImmutableArray<DataTable>.Empty
        };
    }

    #region Private Helpers

    public async Task<EurojackpotProxyResponse?> FetchDataAsync(string url, CancellationToken ct = default)
    {
        using var client = _factory.CreateClient();

        _logger.LogDebug(EvtFetch, "GET {Url}", url);
        var response = await client.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(ct);
        if (string.IsNullOrWhiteSpace(json))
            return null;

        return JsonConvert.DeserializeObject<EurojackpotProxyResponse>(json);
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
    public long? winners { get; set; }
    public long? prize { get; set; }
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

public sealed class EurojackpotProxyResponse
{
    public EurojackpotLatestDraw? last { get; set; }
    public EurojackpotLatestDraw? next { get; set; }
}

public sealed class EurojackpotLatestDraw
{
    public long nr { get; set; }
    public string? currency { get; set; }
    public EurojackpotDate? date { get; set; }
    public string? closingDate { get; set; }
    public string? lateClosingDate { get; set; }
    public string? drawingDate { get; set; }
    public int[]? numbers { get; set; }
    public int[]? euroNumbers { get; set; }
    public string? jackpot { get; set; }
    public string? marketingJackpot { get; set; }
    public string? specialMarketingJackpot { get; set; }
    public long? Winners { get; set; }
    public IDictionary<string, EurojackpotRank>? odds { get; set; }
}

public sealed class EurojackpotDate
{
    public string? full { get; set; }
    public int day { get; set; }
    public int month { get; set; }
    public int year { get; set; }
    public int hour { get; set; }
    public int minute { get; set; }
    public string? dayOfWeek { get; set; }
}

public sealed class EurojackpotRank
{
    public long? winners { get; set; }
    public long? specialPrize { get; set; }
    public long? prize { get; set; }
}
