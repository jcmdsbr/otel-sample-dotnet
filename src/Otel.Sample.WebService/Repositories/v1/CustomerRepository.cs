using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Otel.Sample.SharedKernel.Diagnostics.v1;
using Otel.Sample.SharedKernel.Models.v1;

namespace Otel.Sample.WebService.Repositories.v1;

public interface ICustomerRepository
{
    Task<Guid> AddAsync(CustomerRequest customer, CancellationToken cancellationToken);
}

public class CustomerRepository : ICustomerRepository
{
    private readonly IDistributedCache _distributedCache;
    private readonly IInstrumentation _instrumentation;
    private readonly ILogger<CustomerRepository> _logger;

    public CustomerRepository(
        IInstrumentation instrumentation,
        IDistributedCache distributedCache,
        ILogger<CustomerRepository> logger)
    {
        _logger = logger;
        _distributedCache = distributedCache;
        _instrumentation = instrumentation;
    }

    public async Task<Guid> AddAsync(CustomerRequest customer, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Create a customer key");

        var key = Guid.NewGuid();

        using var activityCache = _instrumentation.ActivitySource.StartActivity("Call redis", ActivityKind.Client);
        await _distributedCache.SetStringAsync(key.ToString(), JsonSerializer.Serialize(customer), cancellationToken);

        return key;
    }
}