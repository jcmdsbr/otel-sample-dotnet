using System.Diagnostics;

namespace Otel.Sample.WebService.Diagnostics.v1;

public class Instrumentation : IDisposable
{
    private const string ActivitySourceName = "WebService";

    public Instrumentation()
    {
        ActivitySource = new ActivitySource(ActivitySourceName);
    }

    public ActivitySource ActivitySource { get; }

    public void Dispose()
    {
        ActivitySource.Dispose();
    }
}