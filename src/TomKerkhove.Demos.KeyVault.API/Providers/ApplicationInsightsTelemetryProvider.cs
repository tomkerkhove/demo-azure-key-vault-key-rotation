using System;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using TomKerkhove.Demos.KeyVault.API.Providers.Interfaces;

namespace TomKerkhove.Demos.KeyVault.API.Providers
{
    public class ApplicationInsightsTelemetryProvider : ITelemetryProvider
    {
        private readonly TelemetryClient telemetryClient;

        public ApplicationInsightsTelemetryProvider()
        {
            var instrumentationKey = Startup.Configuration["ApplicationInsights:InstrumentationKey"];
            var config = new TelemetryConfiguration(instrumentationKey);
            telemetryClient = new TelemetryClient(config);
            TelemetryConfiguration.Active.DisableTelemetry = false;
        }

        public void IncreaseGauge(string gaugeName)
        {
            telemetryClient.TrackMetric(gaugeName, value: 1);
        }

        public void LogException(Exception exception)
        {
            telemetryClient.TrackException(exception);
        }

        public void LogTrace(string traceMessage)
        {
            telemetryClient.TrackTrace(traceMessage);
        }

        public void LogEvent(string eventName)
        {
            telemetryClient.TrackEvent(eventName);
        }
    }
}