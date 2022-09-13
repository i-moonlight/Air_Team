using AirTeamApi.Services.Contract;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AirTeamApi.HealthCheck
{
    public class UriHealthCheck : IHealthCheck
    {
        private readonly IAirTeamClient _airTeamHttpClient;
        public UriHealthCheck(IAirTeamClient airTeamHttpClient)
        {
            _airTeamHttpClient = airTeamHttpClient;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            if (await _airTeamHttpClient.IsConnected())
            {
                return HealthCheckResult.Healthy("A healthy result.");
            }

            return HealthCheckResult.Unhealthy("An unhealthy result.");
        }
    }
}
