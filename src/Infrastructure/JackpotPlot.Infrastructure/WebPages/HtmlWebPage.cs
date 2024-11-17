using HtmlAgilityPack;

namespace JackpotPlot.Infrastructure.WebPages;

public sealed class HtmlWebPage : IHtmlWebPage
{
    private static readonly List<string> UserAgents =
    [
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36",
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/89.0.4389.82 Safari/537.36",

        // Firefox
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:89.0) Gecko/20100101 Firefox/89.0",
        "Mozilla/5.0 (Macintosh; Intel Mac OS X 10.15; rv:85.0) Gecko/20100101 Firefox/85.0",

        // Edge
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36 Edg/91.0.864.59",
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/89.0.4389.82 Safari/537.36 Edg/89.0.774.54",

        // Safari (Mac)
        "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/14.0.3 Safari/605.1.15",
        "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_14_6) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/13.1.2 Safari/605.1.15",

        // Mobile User Agents
        "Mozilla/5.0 (iPhone; CPU iPhone OS 14_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/14.0 Mobile/15A372 Safari/604.1",
        "Mozilla/5.0 (Linux; Android 11; SM-G991U) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.77 Mobile Safari/537.36"
    ];

    private static readonly Random Random = new();

    // Method to get a random User-Agent

    public HtmlDocument Load(string url)
    {
        return Load(new Uri(url));
    }

    public HtmlDocument Load(Uri webPageUrl)
    {
        var htmlWeb = new HtmlWeb
        {
            AutoDetectEncoding = true,
            UserAgent = GetRandomUserAgent() // Set a random User-Agent
        };

        htmlWeb.PreRequest += request =>
        {
            request.CookieContainer = new System.Net.CookieContainer();
            return true;
        };

        return htmlWeb.Load(webPageUrl);
    }

    #region Private helpers
    private static string GetRandomUserAgent()
    {
        var index = Random.Next(UserAgents.Count);
        return UserAgents[index];
    }
    #endregion
}