using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Otel.Sample.SharedKernel.Models.v1;
using Otel.Sample.WorkerService.Diagnostics.v1;

namespace Otel.Sample.WorkerService.Repositories.v1;

public class CustomerRepository
{
    private readonly IDistributedCache _distributedCache;
    private readonly Instrumentation _instrumentation;
    private readonly ILogger<CustomerRepository> _logger;

    public CustomerRepository(ILogger<CustomerRepository> logger, IDistributedCache distributedCache,
        Instrumentation instrumentation)
    {
        _logger = logger;
        _distributedCache = distributedCache;
        _instrumentation = instrumentation;
    }

    public async Task<string> AddAsync(Customer customer, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Create a customer key");

        var key = $"{customer.Name}|{customer.LastName}".GetHashCode().ToString();

        _logger.LogTrace("Call redis");

        using var activityCache = _instrumentation.ActivitySource.StartActivity("Call redis", ActivityKind.Client);
        await _distributedCache.SetStringAsync(key, JsonSerializer.Serialize(customer), cancellationToken);

        return key;
    }
}