using System.Net.Http;

namespace AirTeamApi.Services.Contract
{
    public interface IAirTeamClient
    {
        public Uri? BaseUrl { get; }

        Task<string> SearchByKeyword(string keyword, CancellationToken cancellationToken);
        Task<bool> IsConnected();
    }
}
