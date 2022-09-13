using AirTeamApi.Services.Contract;
using AirTeamApi.Services.Impl;
using AirTeamApi.Settings;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Net.Http;

namespace AirTeamApi
{
    public static class ServiceCollectionExtension
    {
        public static void AddAirTeamOptions(this IServiceCollection services, IConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            services.AddOptions();
            services.Configure<AirTeamSetting>(configuration.GetSection("AirTeamSetting"));
        }

        public static void AddCacheSupport(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            if (webHostEnvironment.IsDevelopment())
            {
                services.AddDistributedMemoryCache();
            }
            else
            {
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = configuration.GetConnectionString("Redis");
                    options.InstanceName = "cache_";
                });
            }
        }

        public static void AddAirTeamHttpClient(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpClient<IAirTeamClient, AirTeamClient>("AirTeamClient", httpClient =>
            {
                httpClient.BaseAddress = new Uri(configuration.GetValue<string>("BaseUrl"));
            }).ConfigurePrimaryHttpMessageHandler(() =>
            {
                return new HttpClientHandler() { UseProxy = false };
            });
        }

        public static void AddAppLogging(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            services.AddLogging(config =>
            {
                if (!webHostEnvironment.IsDevelopment())
                {
                    config.ClearProviders();

                    Log.Logger = new LoggerConfiguration()
                        .MinimumLevel.Warning()
                        .Enrich.WithProperty("HostName", Environment.MachineName)
                        .Enrich.FromLogContext()
                        .WriteTo.Seq(configuration.GetConnectionString("Seq"))
                        .CreateLogger();

                    config.AddSerilog();
                }
            });
        }

    }
}
