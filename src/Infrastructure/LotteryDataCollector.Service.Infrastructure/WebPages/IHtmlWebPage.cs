using HtmlAgilityPack;

namespace LotteryDataCollector.Service.Infrastructure.WebPages;

public interface IHtmlWebPage
{
    HtmlDocument Load(string url);
}