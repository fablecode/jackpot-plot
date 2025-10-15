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

            var url = $"https://www.eurojackpot.org/results?date={drawDate:dd-MM-yyyy}";
            _logger.LogDebug(EvtIterate, "Fetching draw data from {Url}.", url);

            EurojackpotOrgResult? dto = null;
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

    private static EurojackpotResult GetDetails(EurojackpotOrgResult dto)
    {
        var year = int.Parse(dto.date?.year ?? throw new InvalidOperationException("Missing year"));
        var month = int.Parse(dto.date?.month ?? throw new InvalidOperationException("Missing month"));
        var day = int.Parse(dto.date?.day ?? throw new InvalidOperationException("Missing day"));

        var main = dto.numbers?.numbers ?? Array.Empty<int>();
        var extra = dto.numbers?.extraNumbers ?? Array.Empty<int>();

        return new EurojackpotResult
        {
            Date = new DateTime(year, month, day),
            MainNumbers = [.. main],
            EuroNumbers = [.. extra],
            JackpotAmount = dto.jackpot.ToString(),
            PrizeBreakdown = ImmutableArray<DataTable>.Empty
        };
    }

    #region Private Helpers

    public async Task<EurojackpotOrgResult?> FetchDataAsync(string url, CancellationToken ct = default)
    {
        using var client = _factory.CreateClient();

        _logger.LogDebug(EvtFetch, "GET {Url}", url);
        var response = await client.GetAsync(url, ct);

        if (response.StatusCode is HttpStatusCode.MovedPermanently or HttpStatusCode.Found)
        {
            var redirectUrl = response.Headers.Location?.ToString();
            if (string.IsNullOrEmpty(redirectUrl))
            {
                _logger.LogError(EvtFetch,
                    "Received redirect from {Url} but no Location header present.", url);
                throw new InvalidOperationException("No redirect URL found in headers.");
            }

            _logger.LogInformation(EvtFetch,
                "Following redirect from {From} to {To}.", url, redirectUrl);
            return await FetchDataAsync(redirectUrl!, ct);
        }

        var json = await response.Content.ReadAsStringAsync(ct);
        _logger.LogDebug(EvtFetch, "Response {StatusCode} with length {Length} from {Url}.",
            (int)response.StatusCode, json?.Length ?? 0, url);

        // Normalize odd payloads BEFORE binding to a type
        var token = JToken.Parse(json ?? "null");

        if (token.Type == JTokenType.Null)
        {
            _logger.LogInformation(EvtFetch, "Null payload from {Url}.", url);
            return null;
        }

        if (token.Type == JTokenType.Array)
        {
            var arr = (JArray)token;

            if (arr.Count == 0 || arr.All(t => t.Type == JTokenType.Null))
            {
                _logger.LogInformation(EvtFetch,
                    "Array payload with no usable items (count={Count}) from {Url}.", arr.Count, url);
                return null;
            }

            if (arr.Count == 1 && arr[0].Type == JTokenType.Object)
            {
                _logger.LogDebug(EvtFetch,
                    "Single-object array wrapper detected for {Url}; unwrapping.", url);
                token = arr[0];
            }
            else
            {
                _logger.LogWarning(EvtFetch,
                    "Unexpected array payload shape (count={Count}) from {Url}.", arr.Count, url);
                throw new JsonSerializationException("Unexpected array payload shape.");
            }
        }

        if (token.Type != JTokenType.Object)
        {
            _logger.LogWarning(EvtFetch,
                "Unexpected token type {TokenType} from {Url}.", token.Type, url);
            throw new JsonSerializationException($"Unexpected token: {token.Type}");
        }

        try
        {
            var dto = token.ToObject<EurojackpotOrgResult>();
            _logger.LogDebug(EvtFetch, "Successfully deserialized payload from {Url}.", url);
            return dto;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(EvtFetch, ex,
                "Deserialization failed for payload from {Url}.", url);
            throw;
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
