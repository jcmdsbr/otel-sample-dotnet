using System.Diagnostics;

namespace Otel.Sample.WorkerService.Diagnostics.v1;

public class Instrumentation : IDisposable
{
    private const string ActivitySourceName = "WorkService";

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