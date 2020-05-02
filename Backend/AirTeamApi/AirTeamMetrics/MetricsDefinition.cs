using Prometheus;

namespace AirTeamApi.AirTeamMetrics
{
    public static class MetricsDefinition
    {
        public static readonly Counter ApiCallTotal = Metrics.CreateCounter("airimagefind_apicall_total",
            "total times we called our search api",
            new CounterConfiguration() { LabelNames = new string[] { "host" } });

        public static readonly Counter ApiCallOutsideTotal = Metrics.CreateCounter("airimagefind_apicall_outside_total",
            "total times we called airimagesteam.com search api",
            new CounterConfiguration() { LabelNames = new string[] { "host" } });

        public static readonly Counter ApiCallCachedTotal = Metrics.CreateCounter("airimagefind_apicall_cached_total",
            "total times we used cached result",
            new CounterConfiguration() { LabelNames = new string[] { "host" } });

    }
}
