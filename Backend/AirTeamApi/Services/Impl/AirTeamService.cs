using AirTeamApi.Services.Contract;
using AirTeamApi.Services.Models;
using AirTeamApi.Settings;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace AirTeamApi.Services.Impl
{
    public class AirTeamService : IAirTeamService
    {
        private readonly IAirTeamHttpClient _AirTeamHttpClient;
        private readonly IHtmlParseService _HtmlParserService;
        private readonly AirTeamSetting _AirTeamSetting;
        private readonly IDatabase _RedisDatabase;
        private readonly string _CachePrefix = "cache_";

        public AirTeamService(IConnectionMultiplexer connectionMultiplexer, IAirTeamHttpClient httpClient, IHtmlParseService htmlParserService, IOptions<AirTeamSetting> option)
        {
            _AirTeamHttpClient = httpClient;
            _HtmlParserService = htmlParserService;
            
            if (option is null)
            {
                throw new ArgumentNullException(nameof(option));
            }
            _AirTeamSetting = option?.Value;

            _RedisDatabase = connectionMultiplexer?.GetDatabase();
            
            if (connectionMultiplexer == null)
                throw new ArgumentNullException(nameof(connectionMultiplexer));
        }

        public async Task<IEnumerable<ImageDto>> SearchByKeyword(string keyword = "")
        {
            var searchString = keyword?.Trim().ToLower().Replace(" ", "");
            var cacheKey = this._CachePrefix + searchString;

            var redisValue = await _RedisDatabase.StringGetAsync(cacheKey);
            string htmlResponse;
            
            if (redisValue.IsNull)
            {
                var apiResponse = await _AirTeamHttpClient.SearchByKeyword(searchString);
                var responseNode =_HtmlParserService.QuerySelector(apiResponse, "#lb-management-content");

                htmlResponse = responseNode.WriteTo();
                await _RedisDatabase.StringSetAsync(cacheKey, htmlResponse, TimeSpan.FromMinutes(_AirTeamSetting.CacheExprationMinutes));
            }
            else
            {
                htmlResponse = redisValue.ToString();
            }

            var htmlNodes = _HtmlParserService.QuerySelectorAll(htmlResponse, ".thumb");
            var images = htmlNodes.Select(node => getImageFromNode(node));

            return images;
        }


        /*
        <div class="thumb">
		    <div class="image">
			    <div class="id"> Image ID: 332788 </div>
			    <a href="/boeing-777_N779XW_boeing_332788.html"><img src="pics/332/332788_200.jpg" alt="The First test airplane of Boeing 777-9X featuring GE's latest and big..." title="The First test airplane of Boeing 777-9X featuring GE's latest and big..." style="margin-top: 30.5px"></a>
		    </div>
		    <div>General Electric GE9X Engine</div>
	    </div>
        */
        private ImageDto getImageFromNode(HtmlNode node)
        {
            var image = new ImageDto();
            image.ImageId = node.QuerySelector(".id").InnerText.Replace("Image ID:", "").Trim();

            var imageNode = node.QuerySelector("img");
            image.Description = HttpUtility.HtmlDecode(imageNode.Attributes["alt"].Value);
            image.BaseImageUrl = Combine(_AirTeamHttpClient.BaseUrl?.ToString(), imageNode.Attributes["src"].Value);

            image.Title = HttpUtility.HtmlDecode(node.QuerySelector("div:last-child").InnerHtml);
            image.DetailUrl = Combine(_AirTeamHttpClient.BaseUrl?.ToString(), node.QuerySelector("a").Attributes["href"].Value);

            return image;
        }

        private string Combine(string uri1, string uri2)
        {
            uri1 = uri1?.TrimEnd('/');
            uri2 = uri2?.TrimStart('/');
            return $"{uri1}/{uri2}";
        }
    }
}
