using System.Diagnostics;
using System.Net.Http.Json;
using Otel.Sample.SharedKernel.Diagnostics.v1;

namespace Otel.Sample.WorkerService.Clients.v1;

public class CustomerRequest
{
    public string? Name { get; set; }
    public string? LastName { get; set; }
}

public sealed class CustomerClientHandler : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly IInstrumentation _instrumentation;
    private bool _disposedValue;

    public CustomerClientHandler(
        HttpClient httpClient,
        IInstrumentation instrumentation)
    {
        _httpClient = httpClient;
        _instrumentation = instrumentation;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public async Task CreateAsync(CustomerRequest request, CancellationToken cancellation)
    {
        using var activityMain =
            _instrumentation.ActivitySource.StartActivity("Call customer service", ActivityKind.Client);

        await _httpClient.PostAsJsonAsync("/v1/customers", request, cancellation);
    }

    private void Dispose(bool disposing)
    {
        if (_disposedValue) return;

        if (disposing) _httpClient.Dispose();

        _disposedValue = true;
    }
}