using System;
using System.Threading;
using System.Threading.Tasks;

namespace AirTeamApi.Services.Contract
{
    public interface IAirTeamHttpClient
    {
        public Uri? BaseUrl { get; }
        Task<string> SearchByKeyword(string keyword, CancellationToken cancellationToken);
    }
}
