using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using System.Net.Http;
using System.Threading.Tasks;

namespace AirTeamApi.Tests.IntegratedTest
{
    public abstract class BaseIntegrationTest
    {
        public bool InitDone { get; private set; } = false;
        public IHost WebHost { get; private set; } = default!;
        public Mock<IDistributedCache> DistributedCache { get; private set; } = default!;

        public async Task<HttpClient> GetClient()
        {
            if (!InitDone)
            {
                await StartServer();
            }

            return WebHost.GetTestClient();
        }

        public async Task StartServer()
        {
            if (InitDone)
                return;

            DistributedCache = new Mock<IDistributedCache>();

            // Arrange
            var hostBuilder = Host.CreateDefaultBuilder()
                .ConfigureWebHost(webHost =>
                {
                    webHost.UseTestServer();
                    // Add TestServer
                    webHost.UseStartup<Startup>();
                    // Specify the environment
                    // webHost.UseEnvironment("Development");
                    webHost.ConfigureTestServices(services =>
                    {
                        services.AddSingleton(DistributedCache.Object);
                    });
                });

            // Create and start up the host
            WebHost = await hostBuilder.StartAsync();
            InitDone = true;
        }
    }
}
