using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Otel.Sample.Bff.Clients.v1;
using Otel.Sample.Bff.Models.v1;
using Otel.Sample.SharedKernel.Diagnostics.v1;

namespace Otel.Sample.Bff.Endpoints.v1;

public static class Customers
{
    public static WebApplication Register(WebApplication app)
    {
        var customers = app.MapGroup("/v1/customers");

        customers.MapPost(string.Empty, async (
                [FromServices] IInstrumentation instrumentation,
                [FromServices] CustomerClientHandler client,
                [FromBody] CustomerRequest request,
                CancellationToken cancellationToken) =>
            {
                using var activityMain =
                    instrumentation.ActivitySource.StartActivity("Starting create customer flow", ActivityKind.Server);

                var (statusCode, response) = await client.CreateAsync(request, cancellationToken);

                return Results.Content(JsonSerializer.Serialize(response), statusCode: statusCode);
            })
            .WithName("Create Customer")
            .WithOpenApi();

        return app;
    }
}