using AirTeamApi.Services.Contract;
using AirTeamApi.Services.Models;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace AirTeamApi.Services.Impl
{
    public class AirTeamService : IAirTeamService
    {
        private readonly IAirTeamHttpClient _AirTeamHttpClient;
        private readonly IHtmlParseService _HtmlParserService;
        private readonly IDistributedCache _Cache;

        public AirTeamService(IDistributedCache cache, IAirTeamHttpClient httpClient, IHtmlParseService htmlParserService)
        {
            _AirTeamHttpClient = httpClient;
            _HtmlParserService = htmlParserService;
            _Cache = cache;
        }

        public async Task<IEnumerable<ImageDto>> SearchByKeyword(string keyword = "")
        {
            var searchString = keyword?.Trim().ToLower().Replace(" ", "");
            var htmlResponse = _Cache.GetString(searchString);

            if (string.IsNullOrWhiteSpace(htmlResponse))
            {
                htmlResponse = await _AirTeamHttpClient.SearchByKeyword(searchString);
                _Cache.SetString(searchString, htmlResponse, new DistributedCacheEntryOptions() { SlidingExpiration = TimeSpan.FromMinutes(10) });
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
            image.BaseImageUrl = _AirTeamHttpClient.BaseUrl + imageNode.Attributes["src"].Value.Replace("_200.jpg", "");

            image.Title = HttpUtility.HtmlDecode(node.QuerySelector("div:last-child").InnerHtml);
            image.DetailUrl = _AirTeamHttpClient.BaseUrl + node.QuerySelector("a").Attributes["href"].Value;

            return image;
        }
    }
}
