using System.Net.Http;

namespace AirTeamApi.Services.Contract
{
    public interface IAirTeamHttpClient
    {
        HttpClient HttpClient { get; }
        public Uri? BaseUrl { get; }

        Task<string> SearchByKeyword(string keyword, CancellationToken cancellationToken);
    }
}
