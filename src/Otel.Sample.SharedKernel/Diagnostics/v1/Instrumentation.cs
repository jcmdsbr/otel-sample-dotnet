using System.Diagnostics;

namespace Otel.Sample.SharedKernel.Diagnostics.v1;

public interface IInstrumentation : IDisposable
{
    ActivitySource ActivitySource { get; }
}

public sealed class Instrumentation : IInstrumentation
{
    private bool _disposedValue;

    public Instrumentation(string sourceName)
    {
        ActivitySource = new ActivitySource(sourceName);
    }

    public ActivitySource ActivitySource { get; }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_disposedValue) return;

        if (disposing) ActivitySource.Dispose();

        _disposedValue = true;
    }

    ~Instrumentation()
    {
        Dispose(false);
    }
}