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

        const string activityName = "HSET customer";

        using var activityCache = _instrumentation.ActivitySource.StartActivity(activityName, ActivityKind.Client);

        var key = customer.Id.ToString();
        var data = JsonSerializer.Serialize(customer);

        await _distributedCache.SetStringAsync(key, data, cancellationToken);

        // The semantic conventions of the OpenTelemetry Redis specification
        activityCache?.SetTag("db.system", "redis");
        activityCache?.SetTag("net.peer.name", "redis");
        activityCache?.SetTag("net.transport", "Unix");
        activityCache?.SetTag("db.statement", $"HSET  {customer.Id} data {data}");
        activityCache?.SetTag("db.redis.database_index", "0");
    }
}