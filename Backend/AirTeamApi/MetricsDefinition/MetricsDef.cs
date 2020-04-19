using Prometheus;

namespace AirTeamApi.MetricsDefinition
{
    public static class MetricsDef
    {
        public static readonly Counter ApiCallTotal = Metrics.CreateCounter("airimagefind_apicall_total", "total times we called our search api");
        public static readonly Counter ApiCallOutsideTotal = Metrics.CreateCounter("airimagefind_apicall_outside_total", "total times we called airimagesteam.com search api");
        public static readonly Counter ApiCallCachedTotal = Metrics.CreateCounter("airimagefind_apicall_cached_total", "total times we used cached result");
    }
}
