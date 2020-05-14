using AirTeamApi.Services.Contract;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AirTeamApi.Services.Impl
{
    public class AirTeamHttpClient : IAirTeamHttpClient
    {
        private readonly HttpClient _HttpClient;
        public Uri BaseUrl => _HttpClient.BaseAddress;

        public AirTeamHttpClient(HttpClient httpClient)
        {
            _HttpClient = httpClient;
        }

        public async Task<string> SearchByKeyword(string keyword, CancellationToken cancellationToken)
        {
            var httpResponseMessage = await _HttpClient.GetAsync($"search.php?srch_keyword={keyword}", cancellationToken);
            var responseHtmlBody = await httpResponseMessage.Content.ReadAsStringAsync();

            return responseHtmlBody;
        }

    }
}
