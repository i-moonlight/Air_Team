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
    public class AirTeamDataService : IAirTeamDataService
    {
        private readonly IAirTeamClient _airTeamHttpClient;
        private readonly IHtmlParseService _htmlParserService;
        private readonly IDistributedCache _cache;
        private readonly AirTeamSetting _airTeamSetting;

        public AirTeamDataService(IDistributedCache cache, IAirTeamClient httpClient, IHtmlParseService htmlParserService, IOptions<AirTeamSetting> airTeamSetting)
        {
            if (airTeamSetting == null)
                throw new ArgumentNullException(nameof(airTeamSetting));

            _airTeamHttpClient = httpClient;
            _htmlParserService = htmlParserService;
            _cache = cache;
            _airTeamSetting = airTeamSetting.Value;

            if (_airTeamHttpClient.BaseUrl is null)
            {
                throw new ArgumentException(nameof(_airTeamHttpClient.BaseUrl));
            }

        }

        public async Task<IEnumerable<ImageDto>> SearchByKeyword(string keyword)
        {
            if (keyword == null)
                throw new ArgumentNullException(nameof(keyword));

            MetricsDefinition.ApiCallTotal.WithLabels(Environment.MachineName).Inc();

            var searchString = keyword.Trim();
            string cacheString = GetCacheKey(searchString);
            var htmlResponse = await _cache.GetStringAsync(cacheString);

            if (string.IsNullOrWhiteSpace(htmlResponse))
            {
                MetricsDefinition.ApiCallOutsideTotal.WithLabels(Environment.MachineName).Inc();
                htmlResponse = await GetFromAirTeamImages(searchString);
            }
            else
            {
                MetricsDefinition.ApiCallCachedTotal.WithLabels(Environment.MachineName).Inc();
            }

            var imageHtmlNodes = _htmlParserService.QuerySelectorAll(htmlResponse, ".thumb");
            IEnumerable<ImageDto> images = imageHtmlNodes.Select(
                node => GetImageFromNode(node, _airTeamHttpClient.BaseUrl));

            return images;
        }

        private static string GetCacheKey(string searchString)
        {
            return searchString.Replace(" ", "").ToLower();
        }

        private async Task<string> GetFromAirTeamImages(string searchString)
        {
            using CancellationTokenSource httpTokenSource = new(20000);
            var apiResponse = await _airTeamHttpClient.SearchByKeyword(searchString, httpTokenSource.Token);

            var resultDivision = _htmlParserService.QuerySelector(apiResponse, "#lb-management-content");
            var resultHtml = resultDivision.WriteTo();

            var cacheEntryOption = new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_airTeamSetting.CacheExprationMinutes)
            };

            var cacheKey = GetCacheKey(searchString);
            await _cache.SetStringAsync(cacheKey, resultHtml, cacheEntryOption);
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
