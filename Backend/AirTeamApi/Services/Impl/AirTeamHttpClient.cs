using AirTeamApi.Services.Contract;
using System.Net.Http;

namespace AirTeamApi.Services.Impl
{
    public class AirTeamHttpClient : IAirTeamHttpClient
    {
        public HttpClient HttpClient { get; }
        public Uri? BaseUrl => HttpClient?.BaseAddress;

        public AirTeamHttpClient(HttpClient httpClient)
        {
            HttpClient = httpClient;
        }

        public async Task<string> SearchByKeyword(string keyword, CancellationToken cancellationToken)
        {
            using var httpResponseMessage = await HttpClient.GetAsync($"search.php?srch_keyword={keyword}", HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            httpResponseMessage.EnsureSuccessStatusCode();
            var responseHtmlBody = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);

            return responseHtmlBody;
        }

    }
}
