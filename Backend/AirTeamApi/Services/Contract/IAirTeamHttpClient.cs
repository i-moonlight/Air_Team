using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AirTeamApi.Services.Contract
{
    public interface IAirTeamHttpClient
    {
        HttpClient HttpClient { get; }
        public Uri? BaseUrl { get; }

        Task<string> SearchByKeyword(string keyword, CancellationToken cancellationToken);
    }
}
