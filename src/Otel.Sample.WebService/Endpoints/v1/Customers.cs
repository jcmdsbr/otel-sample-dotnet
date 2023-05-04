using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Otel.Sample.SharedKernel.Diagnostics.v1;
using Otel.Sample.WebService.Handlers.v1;
using Otel.Sample.WebService.Models.v1;
using Otel.Sample.WebService.Repositories.v1;

namespace Otel.Sample.WebService.Endpoints.v1;

public static class Customers
{
    public static WebApplication Register(WebApplication app)
    {
        var customers = app.MapGroup("/v1/customers");

        customers.MapPost(string.Empty, async (
                [FromServices] ICustomerRepository repository,
                [FromServices] IMessageSenderHandler sender,
                [FromServices] IInstrumentation instrumentation,
                [FromBody] CustomerRequest request,
                CancellationToken cancellationToken) =>
            {
                const string activityName = "Starting create customer process";
                using var activityMain =
                    instrumentation.ActivitySource.StartActivity(activityName, ActivityKind.Server);
                var customerCreatedCounter = instrumentation.MeterSource.CreateCounter<int>("customer-created");

                var (name, lastname) = request;
                var customer = new Customer(Guid.NewGuid(), name, lastname, DateTime.Now);

                await repository.AddAsync(customer, cancellationToken);
                await sender.SendMessageAsync(customer, cancellationToken);

                var tags = new[]
                {
                    new KeyValuePair<string, object?>(nameof(customer.Id), customer.Id),
                    new KeyValuePair<string, object?>(nameof(customer.Birthday), customer.Birthday),
                    new KeyValuePair<string, object?>(nameof(customer.Name), $"{customer.Name} {customer.LastName}")
                };

                customerCreatedCounter.Add(1, tags);

                return Results.Created($"/v1/customers/{customer.Id}", new
                {
                    content = request,
                    traceId = activityMain?.TraceId.ToString()
                });
            })
            .WithName("Create Customer")
            .WithOpenApi();

        return app;
    }
}