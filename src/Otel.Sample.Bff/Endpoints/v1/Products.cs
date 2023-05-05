using System.Diagnostics;
using Grpc.Core;
using Microsoft.AspNetCore.Mvc;
using Otel.Sample.Bff.Models.v1;
using Otel.Sample.gRPC;
using Otel.Sample.SharedKernel.Diagnostics.v1;

namespace Otel.Sample.Bff.Endpoints.v1;

public static class Products
{
    public static WebApplication Register(WebApplication app)
    {
        var products = app.MapGroup("/v1/products");

        products.MapGet(string.Empty, async (
                [FromServices] IInstrumentation instrumentation,
                [FromServices] Product.ProductClient client,
                CancellationToken cancellationToken) =>
            {
                using var activityMain =
                    instrumentation.ActivitySource.StartActivity("Starting find products flow", ActivityKind.Server);

                try
                {
                    var response =
                        await client.FindAsync(new GetProductRequest(), cancellationToken: cancellationToken);

                    return Results.Ok(new { TraceId = activityMain?.TraceId.ToString(), Content = response });
                }
                catch (RpcException ex)
                {
                    return Results.Problem(ex.Message);
                }
            })
            .WithName("Get Products")
            .WithOpenApi();

        products.MapPost(string.Empty, async (
                [FromServices] IInstrumentation instrumentation,
                [FromServices] Product.ProductClient client,
                [FromBody] ProductRequest request,
                CancellationToken cancellationToken) =>
            {
                using var activityMain =
                    instrumentation.ActivitySource.StartActivity("Starting create productEntity flow",
                        ActivityKind.Server);

                try
                {
                    var response = await client.CreateAsync(new CreateProductRequest { Name = request.Name },
                        cancellationToken: cancellationToken);

                    return Results.Created($"/v1/products/{response.Id}",
                        new { TraceId = activityMain?.TraceId.ToString(), Content = response });
                }

                catch (RpcException ex)
                {
                    return Results.Problem(ex.Message);
                }
            })
            .WithName("Create Products")
            .WithOpenApi();


        return app;
    }
}