using AirTeamApi.AirTeamMetrics;
using AirTeamApi.Services.Contract;
using AirTeamApi.Services.Models;
using AirTeamApi.Settings;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System.Web;

namespace AirTeamApi.Services.Impl
{
    public class AirTeamService : IAirTeamService
    {
        private readonly IAirTeamHttpClient _AirTeamHttpClient;
        private readonly IHtmlParseService _HtmlParserService;
        private readonly IDistributedCache _Cache;
        private readonly AirTeamSetting _AirTeamSetting;

        public AirTeamService(IDistributedCache cache, IAirTeamHttpClient httpClient, IHtmlParseService htmlParserService, IOptions<AirTeamSetting> airTeamSetting)
        {
            if (airTeamSetting == null)
                throw new ArgumentNullException(nameof(airTeamSetting));

            _AirTeamHttpClient = httpClient;
            _HtmlParserService = htmlParserService;
            _Cache = cache;
            _AirTeamSetting = airTeamSetting.Value;

            if (_AirTeamHttpClient.BaseUrl is null)
            {
                throw new ArgumentException(nameof(_AirTeamHttpClient.BaseUrl));
            }

        }

        public async Task<IEnumerable<ImageDto>> SearchByKeyword(string keyword)
        {
            if (keyword == null)
                throw new ArgumentNullException(nameof(keyword));

            MetricsDefinition.ApiCallTotal.WithLabels(Environment.MachineName).Inc();

            var searchString = keyword.Trim();
            string cacheString = GetCacheKey(searchString);
            var htmlResponse = await _Cache.GetStringAsync(cacheString);

            if (string.IsNullOrWhiteSpace(htmlResponse))
            {
                MetricsDefinition.ApiCallOutsideTotal.WithLabels(Environment.MachineName).Inc();
                htmlResponse = await GetFromAirTeamImages(searchString);
            }
            else
            {
                MetricsDefinition.ApiCallCachedTotal.WithLabels(Environment.MachineName).Inc();
            }

            var imageHtmlNodes = _HtmlParserService.QuerySelectorAll(htmlResponse, ".thumb");
            IEnumerable<ImageDto> images = imageHtmlNodes.Select(node => GetImageFromNode(node, _AirTeamHttpClient.BaseUrl));

            return images;
        }

        private static string GetCacheKey(string searchString)
        {
            return searchString.Replace(" ", "").ToLower();
        }

        private async Task<string> GetFromAirTeamImages(string searchString)
        {
            using CancellationTokenSource httpTokenSource = new(20000);
            var apiResponse = await _AirTeamHttpClient.SearchByKeyword(searchString, httpTokenSource.Token);

            var resultDivision = _HtmlParserService.QuerySelector(apiResponse, "#lb-management-content");
            var resultHtml = resultDivision.WriteTo();

            var cacheEntryOption = new DistributedCacheEntryOptions() { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_AirTeamSetting.CacheExprationMinutes) };
            var cacheKey = GetCacheKey(searchString);
            await _Cache.SetStringAsync(cacheKey, resultHtml, cacheEntryOption);
            return resultHtml;
        }

        private static ImageDto GetImageFromNode(HtmlNode node, Uri? baseUrl)
        {
            var image = new ImageDto
            {
                ImageId = node.QuerySelector(".id").InnerText.Replace("Image ID:", "", StringComparison.InvariantCultureIgnoreCase).Trim()
            };

            if (baseUrl is null)
            {
                return image;
            }

            var imageNode = node.QuerySelector("img");
            image.Description = HttpUtility.HtmlDecode(imageNode.Attributes["alt"].Value);
            image.BaseImageUrl = Combine(baseUrl.ToString(), imageNode.Attributes["src"].Value);

            image.Title = HttpUtility.HtmlDecode(node.QuerySelector("div:last-child").InnerHtml);
            image.DetailUrl = Combine(baseUrl.ToString(), node.QuerySelector("a").Attributes["href"].Value);

            return image;
        }

        private static string Combine(string? uri1, string? uri2)
        {
            uri1 = uri1?.TrimEnd('/');
            uri2 = uri2?.TrimStart('/');
            return $"{uri1}/{uri2}";
        }
    }
}
