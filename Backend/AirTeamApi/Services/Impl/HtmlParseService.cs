using AirTeamApi.Services.Contract;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using System.Collections.Generic;

namespace AirTeamApi.Services.Impl
{
    public class HtmlParseService : IHtmlParseService
    {
        public HtmlNode QuerySelector(string htmlString, string query)
        {
            var html = new HtmlDocument();
            html.LoadHtml(htmlString);

            var document = html.DocumentNode;

            var node = document.QuerySelector(query);
            return node;
        }
        public IEnumerable<HtmlNode> QuerySelectorAll(string htmlString, string query)
        {
            var html = new HtmlDocument();
            html.LoadHtml(htmlString);

            var document = html.DocumentNode;

            var nodes = document.QuerySelectorAll(query);
            return nodes;
        }
    }
}
