using OpenTelemetry.Resources;

namespace Otel.Sample.SharedKernel.Helpers.v1;

public static class OTelHelper
{
    public static ResourceBuilder Create(string applicationName)
    {
        return ResourceBuilder
            .CreateDefault()
            .AddService(applicationName)
            .AddTelemetrySdk();
    }
}