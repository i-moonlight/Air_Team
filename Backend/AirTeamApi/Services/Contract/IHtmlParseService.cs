using HtmlAgilityPack;

namespace AirTeamApi.Services.Contract
{
    public interface IHtmlParseService
    {
        HtmlNode QuerySelector(string htmlString, string query);
        IEnumerable<HtmlNode> QuerySelectorAll(string htmlString, string query);
    }
}
