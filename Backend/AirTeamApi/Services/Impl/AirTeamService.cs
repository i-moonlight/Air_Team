using AirTeamApi.Services.Contract;
using AirTeamApi.Services.Models;
using AirTeamApi.Settings;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
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
        private readonly IDistributedCache _Cache;
        private readonly AirTeamSetting _AirTeamSetting;

        public AirTeamService(IDistributedCache cache, IAirTeamHttpClient httpClient, IHtmlParseService htmlParserService, IOptions<AirTeamSetting> option)
        {
            _AirTeamHttpClient = httpClient;
            _HtmlParserService = htmlParserService;
            _Cache = cache;

            if (option is null)
            {
                throw new ArgumentNullException(nameof(option));
            }
            _AirTeamSetting = option?.Value;
        }

        public async Task<IEnumerable<ImageDto>> SearchByKeyword(string keyword = "")
        {
            var searchString = keyword?.Trim().ToLower().Replace(" ", "");
            var htmlResponse = await _Cache.GetStringAsync(searchString, CancellationToken.None);

            if (string.IsNullOrWhiteSpace(htmlResponse))
            {
                var apiResponse = await _AirTeamHttpClient.SearchByKeyword(searchString);

                var responseNode =_HtmlParserService.QuerySelector(apiResponse, "#lb-management-content");
                htmlResponse = responseNode.WriteTo();

                var cacheEntryOption = new DistributedCacheEntryOptions() { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_AirTeamSetting.CacheExprationMinutes) };
                await _Cache.SetStringAsync(searchString, htmlResponse, cacheEntryOption, CancellationToken.None);
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
