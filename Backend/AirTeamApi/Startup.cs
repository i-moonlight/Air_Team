using AirTeamApi.HealthCheck;
using AirTeamApi.Services.Contract;
using AirTeamApi.Services.Impl;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Prometheus;
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
        public IWebHostEnvironment WebHostEnvironment { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAirTeamOptions(Configuration);

            services.AddControllers();

            services.AddCors(c => c.AddPolicy("AllowOrigin", options => options.AllowAnyOrigin()));

            services.AddCacheSupport(Configuration, WebHostEnvironment);

            services.AddSwaggerGen(c => c.SwaggerDoc("v1", new OpenApiInfo { Title = "api doc v1", Version = "v1" }));

            services.AddAirTeamHttpClient(Configuration);

            services.AddAppLogging(Configuration, WebHostEnvironment);
            
            services.AddHealthChecks()
                .AddRedis(Configuration.GetConnectionString("Redis"))
                .AddCheck<UriHealthCheck>("airteamimages.com_site");
            
            services.AddSingleton(Configuration);

            services.AddTransient<IAirTeamService, AirTeamService>();
            services.AddTransient<IHtmlParseService, HtmlParseService>();

        }

        public void Configure(IApplicationBuilder app)
        {
            if (WebHostEnvironment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.UseHttpsRedirection();
            Metrics.SuppressDefaultMetrics();

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "api doc v1"));

            app.UseRouting();
            //app.UseAuthorization();
            app.UseCors(options => options.AllowAnyOrigin());


            app.Use((context, next) =>
            {
                context.Response.Headers["Host-Name"] = Environment.MachineName;
                return next.Invoke();
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health");
                endpoints.MapMetrics("/metrics");
            });


        }

    }
}
