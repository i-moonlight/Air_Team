using HtmlAgilityPack;
using System.Collections.Generic;

namespace AirTeamApi.Services.Contract
{
    public interface IHtmlParseService
    {
        IEnumerable<HtmlNode> QuerySelectorAll(string htmlString, string query);
    }
}
