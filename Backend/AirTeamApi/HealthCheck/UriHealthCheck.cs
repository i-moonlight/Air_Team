using AirTeamApi.Services.Contract;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AirTeamApi.HealthCheck
{
    public class UriHealthCheck : IHealthCheck
    {
        private readonly IAirTeamHttpClient _airTeamHttpClient;
        public UriHealthCheck(IAirTeamHttpClient airTeamHttpClient)
        {
            _airTeamHttpClient = airTeamHttpClient;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                using var httpResponseMessage = await _airTeamHttpClient.HttpClient.GetAsync("/", cancellationToken);

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    return HealthCheckResult.Healthy("A healthy result.");
                }
            }
            catch
            {
                return HealthCheckResult.Unhealthy("An unhealthy result.");
            }

            return HealthCheckResult.Unhealthy("An unhealthy result.");

        }
    }
}
