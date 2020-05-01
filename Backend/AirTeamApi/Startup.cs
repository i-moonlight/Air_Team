using AirTeamApi.HealthCheck;
using AirTeamApi.Services.Contract;
using AirTeamApi.Services.Impl;
using AirTeamApi.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Prometheus;
using Serilog;
using System;

namespace AirTeamApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddOptions();

            // Add our Config object so it can be injected
            services.Configure<AirTeamSetting>(Configuration.GetSection("AirTeamSetting"));

            // *If* you need access to generic IConfiguration this is **required**
            services.AddSingleton<IConfiguration>(Configuration);

            services.AddCors(c =>
            {
                c.AddPolicy("AllowOrigin", options => options.AllowAnyOrigin());
            });

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = Configuration.GetConnectionString("Redis");
                options.InstanceName = "cache_";
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
            });

            services.AddHttpClient<IAirTeamHttpClient, AirTeamHttpClient>("AirTeamClient", httpClient =>
            {
                httpClient.BaseAddress = new Uri(Configuration.GetValue<string>("BaseUrl"));
            });

            services.AddTransient<IAirTeamService, AirTeamService>();
            services.AddTransient<IHtmlParseService, HtmlParseService>();

            services.AddHealthChecks()
                .AddRedis(Configuration.GetConnectionString("Redis"))
                .AddCheck<UriHealthCheck>("airteamimages.com_site");

            services.AddLogging(config =>
            {
                // clear out default configuration
                config.ClearProviders();
                config.AddDebug();
                
                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == Environments.Development)
                {
                    config.AddConsole();
                }
                else
                {
                    Log.Logger = new LoggerConfiguration()
                        .MinimumLevel.Warning()
                        .Enrich.WithProperty("HostName", Environment.MachineName)
                        .Enrich.FromLogContext()
                        .WriteTo.Seq(Configuration.GetConnectionString("Seq"))
                        .CreateLogger();
                    
                    config.AddSerilog();
                }
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.UseHttpsRedirection();

            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });

            app.UseRouting();
            //app.UseAuthorization();

            app.Use((context, next) =>
            {
                context.Response.Headers["Host-Name"] = Environment.MachineName;
                return next.Invoke();
            });

            app.UseCors(options => options.AllowAnyOrigin());

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health");
                endpoints.MapMetrics("/metrics");
            });

            Metrics.SuppressDefaultMetrics();
        }

    }
}
