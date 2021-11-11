using AirTeamApi.HealthCheck;
using AirTeamApi.Services.Contract;
using AirTeamApi.Services.Impl;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Prometheus;
using System.IO;
using System.Text;
using System.Text.Json;

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

            services.AddCors(c => c.AddDefaultPolicy(options =>
                options.AllowAnyOrigin().AllowAnyHeader().DisallowCredentials()));

            services.AddCacheSupport(Configuration, WebHostEnvironment);

            services.AddSwaggerGen(c => c.SwaggerDoc("v1", new OpenApiInfo { Title = "api doc v1", Version = "v1" }));

            services.AddAirTeamHttpClient(Configuration);

            services.AddAppLogging(Configuration, WebHostEnvironment);

            services.AddHealthChecks()
                .AddRedis(Configuration.GetConnectionString("Redis"))
                .AddCheck<UriHealthCheck>("airteamimages.com_site");

            services.AddSingleton(Configuration);

            services.AddScoped<IAirTeamService, AirTeamService>();
            services.AddScoped<IHtmlParseService, HtmlParseService>();

        }

        public void Configure(IApplicationBuilder app)
        {
            if (WebHostEnvironment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.UseHttpsRedirection();
            Metrics.SuppressDefaultMetrics();

            app.UseCors();
            app.UseRouting();
            //app.UseAuthorization();

            app.Use((context, next) =>
            {
                context.Response.Headers["Host-Name"] = Environment.MachineName;
                return next.Invoke();
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapMetrics("/metrics");
                endpoints.MapHealthChecks("/health", new HealthCheckOptions()
                {
                    ResponseWriter = WriteResponse
                });
            });

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "api doc v1"));

        }

        private static Task WriteResponse(HttpContext context, HealthReport result)
        {
            context.Response.ContentType = "application/json; charset=utf-8";

            var options = new JsonWriterOptions
            {
                Indented = true
            };

            using var stream = new MemoryStream();
            using (var writer = new Utf8JsonWriter(stream, options))
            {
                writer.WriteStartObject();
                writer.WriteString("status", result.Status.ToString());
                writer.WriteStartObject("results");
                foreach (var entry in result.Entries)
                {
                    writer.WriteStartObject(entry.Key);
                    writer.WriteString("status", entry.Value.Status.ToString());
                    writer.WriteString("description", entry.Value.Description);
                    writer.WriteStartObject("data");
                    foreach (var item in entry.Value.Data)
                    {
                        writer.WritePropertyName(item.Key);
                        JsonSerializer.Serialize(
                            writer, item.Value, item.Value?.GetType() ??
                            typeof(object));
                    }
                    writer.WriteEndObject();
                    writer.WriteEndObject();
                }
                writer.WriteEndObject();
                writer.WriteEndObject();
            }

            var json = Encoding.UTF8.GetString(stream.ToArray());

            return context.Response.WriteAsync(json);
        }

    }
}
