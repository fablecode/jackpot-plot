using HtmlAgilityPack;

namespace JackpotPlot.Infrastructure.WebPages;

public interface IHtmlWebPage
{
    HtmlDocument Load(string url);
}