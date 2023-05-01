using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Otel.Sample.SharedKernel.Diagnostics.v1;
using Otel.Sample.WebService.Models.v1;

namespace Otel.Sample.WebService.Repositories.v1;

public interface ICustomerRepository
{
    Task AddAsync(Customer customer, CancellationToken cancellationToken);
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

    public async Task AddAsync(Customer customer, CancellationToken cancellationToken)
    {
        var (id, name, lastname, birthday) = customer;
        _logger.LogInformation("Customer to save: {customerId}, {customerName}, {customerLastName}, {birthday}", id,
            name, lastname, birthday);
        using var activityCache =
            _instrumentation.ActivitySource.StartActivity("Saving customer in cache", ActivityKind.Client);
        await _distributedCache.SetStringAsync(customer.Id.ToString(), JsonSerializer.Serialize(customer),
            cancellationToken);
    }
}