using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AirTeamApi.HealthCheck
{
    public class UriHealthCheck : IHealthCheck
    {
        private readonly IConfiguration _Configuration;
        public UriHealthCheck(IConfiguration configuration)
        {
            _Configuration = configuration;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            using var httpClient = new HttpClient()
            {
                Timeout = TimeSpan.FromSeconds(5),
                BaseAddress = new Uri(_Configuration.GetValue<string>("BaseUrl"))
            };

            using var httpResponseMessage = await httpClient.GetAsync("/");

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                return HealthCheckResult.Healthy("A healthy result.");
            }

            return HealthCheckResult.Unhealthy("An unhealthy result.");

        }
    }
}
