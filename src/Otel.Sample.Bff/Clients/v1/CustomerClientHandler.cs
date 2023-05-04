using System.Diagnostics;
using Otel.Sample.Bff.Models.v1;
using Otel.Sample.SharedKernel.Diagnostics.v1;

namespace Otel.Sample.Bff.Clients.v1;

public sealed class CustomerClientHandler : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly IInstrumentation _instrumentation;
    private readonly ILogger<CustomerClientHandler> _logger;
    private bool _disposedValue;

    public CustomerClientHandler(
        HttpClient httpClient,
        IInstrumentation instrumentation,
        ILogger<CustomerClientHandler> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _instrumentation = instrumentation;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public async Task<(int, Response)> CreateAsync(CustomerRequest request, CancellationToken cancellation)
    {
        using var activityMain =
            _instrumentation.ActivitySource.StartActivity("Call customer service", ActivityKind.Client);

        var customerSuccessHistogram =
            _instrumentation.MeterSource.CreateHistogram<long>("customer-success-process-ms");
        var customerFailHistogram = _instrumentation.MeterSource.CreateHistogram<long>("customer-fail-process-ms");

        var traceId = activityMain?.TraceId.ToString();
        var ms = new Stopwatch();

        ms.Start();
        var httpResponse = await _httpClient.PostAsJsonAsync("/v1/customers", request, cancellation);
        Thread.Sleep(new Random().Next(100, 200));
        ms.Stop();


        if (httpResponse.IsSuccessStatusCode)
        {
            customerSuccessHistogram.Record(ms.ElapsedMilliseconds);
            return ((int)httpResponse.StatusCode,
                new Response(new { TraceId = traceId }, new[] { "Success !! customer created." }));
        }


        var error = await httpResponse.Content.ReadAsStringAsync(cancellation);

        _logger.LogError("Error when call customer service: {response}", error);

        customerFailHistogram.Record(ms.ElapsedMilliseconds);

        return ((int)httpResponse.StatusCode,
            new Response(new { TraceId = traceId }, new[] { "Failed!! to create a customer." }));
    }

    private void Dispose(bool disposing)
    {
        if (_disposedValue) return;

        if (disposing) _httpClient.Dispose();

        _disposedValue = true;
    }
}