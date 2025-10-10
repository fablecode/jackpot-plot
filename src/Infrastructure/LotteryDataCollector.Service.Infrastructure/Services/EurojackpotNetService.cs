using System.Collections.Immutable;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using JackpotPlot.Application.Abstractions.Services;
using JackpotPlot.Domain.Models;
using LotteryDataCollector.Service.Infrastructure.WebPages;
using Microsoft.Extensions.Logging;

namespace LotteryDataCollector.Service.Infrastructure.Services;

public class EurojackpotNetService : IEurojackpotService
{
    private readonly ILogger<EurojackpotNetService> _logger;
    private readonly IHtmlWebPage _htmlWebPage;

    public EurojackpotNetService(ILogger<EurojackpotNetService> logger, IHtmlWebPage htmlWebPage)
    {
        _logger = logger;
        _htmlWebPage = htmlWebPage;
    }
    public IEnumerable<EurojackpotResult> GetAllDrawHistoryResults()
    {
        const string baseUrl = "https://www.euro-jackpot.net/results-archive";

        for (var year = 2012; year <= DateTime.Now.Year; year++)
        {
            var url = $"{baseUrl}-{year}";

            var drawUrls = ExtractResultLinksAsync(url);

            foreach (var drawUrl in drawUrls)
            {
                var drawDetails = GetDrawDetails(drawUrl);

                yield return drawDetails;
            }
        }
    }

    public IAsyncEnumerable<EurojackpotResult> GetAllDrawHistoryResultsAsync()
    {
        throw new NotImplementedException();
    }

    public EurojackpotResult GetDrawDetails(string drawUrl)
    {
        var draw = new EurojackpotResult();

        var drawHtmlDoc = _htmlWebPage.Load(drawUrl);

        // Draw date
        draw.Date = ExtractDrawDateTime(drawUrl);

        _logger.LogInformation("Retrieving eurojackpot draw details for {date}.", draw.Date);

        // Rollover
        var rolloverNode = drawHtmlDoc.DocumentNode.SelectSingleNode("//div[@class=\"bright-box results breakdown euro-jackpot\"]//div[@class='ribbon fx align--center']//span[1]");
        if (rolloverNode != null)
        {
            draw.Rollover = ExtractRollover(rolloverNode.InnerText.TrimEnd());
        }

        var mainNumbersNode = drawHtmlDoc.DocumentNode.SelectSingleNode("//ul[contains(@class, 'balls')]");
        if (mainNumbersNode != null)
        {
            draw.MainNumbers = [..mainNumbersNode.SelectNodes(".//li[@class='ball']").Select(n => int.Parse(n.InnerText.Trim()))];
        }

        var euroNumbersNode = drawHtmlDoc.DocumentNode.SelectSingleNode("//ul[contains(@class, 'balls')]");
        if (euroNumbersNode != null)
        {
            draw.EuroNumbers = euroNumbersNode.SelectNodes(".//li[@class='euro']").Select(n => int.Parse(n.InnerText.Trim())).ToImmutableArray();
        }

        var jackpotWinnersNode = drawHtmlDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'box winners')]//div[@class='elem2']");
        if (jackpotWinnersNode != null)
        {
            draw.JackpotWinners = int.Parse(jackpotWinnersNode.InnerText.Trim());
        }

        var totalWinnersNode = drawHtmlDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'box total-winners')]//div[@class='elem2']");
        if (totalWinnersNode != null)
        {
            draw.TotalWinners = int.Parse(totalWinnersNode.InnerText.Trim(), NumberStyles.AllowThousands);
        }

        var jackpotAmountNode = drawHtmlDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'box jackpot-amount')]//div[@class='elem2']");
        if (jackpotAmountNode != null)
        {
            draw.JackpotAmount = jackpotAmountNode.InnerText.Trim();
        }

        var tableNodes = drawHtmlDoc.DocumentNode.SelectNodes("//table[@class='table-alt'] | //table[@class='prize-breakdown']");
        if (tableNodes != null)
        {
            draw.PrizeBreakdown = ExtractPrizeBreakdown(tableNodes);
        }

        return draw;
    }

    public IEnumerable<string> ExtractResultLinksAsync(string url)
    {
        var htmlDoc = _htmlWebPage.Load(url);

        // Locate the table rows containing the result links
        var rows = htmlDoc.DocumentNode.SelectNodes("//table//tbody//tr");

        if (rows != null)
        {
            // Iterate through the rows from bottom to top
            for (int i = rows.Count - 1; i >= 0; i--)
            {
                var row = rows[i];
                var linkNode = row.SelectSingleNode(".//td[1]//a"); // Assuming the link is in the first column (Result Date)

                if (linkNode != null)
                {
                    var href = linkNode.GetAttributeValue("href", null);
                    if (!string.IsNullOrEmpty(href))
                    {
                        // Convert relative URLs to absolute URLs if needed
                        if (!href.StartsWith("http"))
                        {
                            href = new Uri(new Uri(url), href).ToString();

                            yield return href;
                        }
                    }
                }
            }
        }
    }

    #region Private Helpers

    public static ImmutableArray<DataTable> ExtractPrizeBreakdown(HtmlNodeCollection tableNodes)
    {
        var dataTables = new List<DataTable>();

        foreach (var tableNode in tableNodes)
        {
            var dataTable = new DataTable();

            // Extract headers
            var headers = tableNode.SelectNodes(".//thead//th")
                ?.Select(th => th.InnerText.Trim())
                .ToArray();

            if (headers != null)
            {
                dataTable.Columns.AddRange(headers.Select(header => new DataColumn(header)).ToArray());
            }

            // Extract rows
            var rows = tableNode.SelectNodes(".//tbody//tr")
                ?.Select(tr => tr.SelectNodes("td")
                    ?.Select(td => td.InnerText.Trim())
                    .ToArray())
                .Where(row => row != null)
                .ToList();

            // Add rows to the DataTable
            rows?.ForEach(row => dataTable.Rows.Add(row));

            dataTables.Add(dataTable);
        }

        return dataTables.ToImmutableArray();
    }

    public static DateTime ExtractDrawDateTime(string url)
    {
        var match = Regex.Match(url, @"\d{2}-\d{2}-\d{4}$");

        if (match.Success)
        {
            var dateString = match.Value; // Extracted date part as string
            return DateTime.ParseExact(dateString, "dd-MM-yyyy", CultureInfo.InvariantCulture);
        }

        throw new FormatException("Failed to extract date.");
    }

    public static int ExtractRollover(string text)
    {
        var match = Regex.Match(text, @"(\d+)(?=&times;)"); // Match one or more digits

        return match.Success
            ? int.Parse(match.Groups[1].Value)
            : default;
    }

    #endregion
}