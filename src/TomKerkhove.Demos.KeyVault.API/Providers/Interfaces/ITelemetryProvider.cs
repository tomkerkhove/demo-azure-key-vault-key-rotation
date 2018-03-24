using System;

namespace TomKerkhove.Demos.KeyVault.API.Providers.Interfaces
{
    public interface ITelemetryProvider
    {
        void IncreaseGauge(string gaugeName);
        void LogException(Exception exception);
        void LogTrace(string traceMessage);
    }
}