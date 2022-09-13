using AirTeamApi.Services.Contract;
using System.Net.Http;
using System.Threading;

namespace AirTeamApi.Services.Impl
{
    public class AirTeamClient : IAirTeamClient
    {
        private readonly HttpClient _httpClient;
        public Uri? BaseUrl => _httpClient?.BaseAddress;

        public AirTeamClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> SearchByKeyword(string keyword, CancellationToken cancellationToken)
        {
            using var httpResponseMessage = await _httpClient.GetAsync($"search.php?srch_keyword={keyword}", HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            httpResponseMessage.EnsureSuccessStatusCode();
            var responseHtmlBody = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);

            return responseHtmlBody;
        }

        public async Task<bool> IsConnected()
        {
            try
            {
                using var httpResponseMessage = await this._httpClient.GetAsync("/", CancellationToken.None);
                return httpResponseMessage.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

    }
}
