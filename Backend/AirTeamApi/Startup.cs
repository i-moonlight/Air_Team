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
        public Startup(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            Configuration = configuration;
            WebHostEnvironment = webHostEnvironment;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment  WebHostEnvironment { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddOptions();

            services.Configure<AirTeamSetting>(Configuration.GetSection("AirTeamSetting"));

            services.AddSingleton(Configuration);

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
                if (!WebHostEnvironment.IsDevelopment())
                {
                    config.ClearProviders();

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
        public void Configure(IApplicationBuilder app)
        {
            if (WebHostEnvironment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.UseHttpsRedirection();

            app.UseSwagger();

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
