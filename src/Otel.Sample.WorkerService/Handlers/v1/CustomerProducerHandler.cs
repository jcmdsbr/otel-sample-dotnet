using System.Diagnostics;
using Bogus;
using Otel.Sample.SharedKernel.Diagnostics.v1;
using Otel.Sample.WorkerService.Clients.v1;

namespace Otel.Sample.WorkerService.Handlers.v1;

public interface ICustomerProducerHandler
{
    Task StartAsync(CancellationToken cancellationToken);
}

public class CustomerProducerHandler : ICustomerProducerHandler
{
    private readonly CustomerClientHandler _customerClientHandler;
    private readonly IInstrumentation _instrumentation;

    public CustomerProducerHandler(CustomerClientHandler customerClientHandler, IInstrumentation instrumentation)
    {
        _customerClientHandler = customerClientHandler;
        _instrumentation = instrumentation;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var activityMain =
            _instrumentation.ActivitySource.StartActivity("Call customer service", ActivityKind.Client);

        var testCustomer = new Faker<CustomerRequest>()
            .RuleFor(x => x.Name, x => x.Name.FirstName())
            .RuleFor(x => x.LastName, x => x.Name.LastName());

        await _customerClientHandler.CreateAsync(testCustomer.Generate(), cancellationToken);
    }
}