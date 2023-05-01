using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Resources;
using Otel.Sample.Bff.Clients.v1;
using Otel.Sample.Bff.Models.v1;
using Otel.Sample.gRPC;
using Otel.Sample.SharedKernel;
using Otel.Sample.SharedKernel.Diagnostics.v1;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

var applicationName = "BffServoce";
var applicationVersion = "v1";
var applicationNamespace = "Otel.Sample";

var resourceBuilder = ResourceBuilder
    .CreateDefault()
    .AddService(applicationName, applicationNamespace, applicationVersion)
    .AddTelemetrySdk();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOtel(configuration, resourceBuilder, applicationName);
builder.Logging.AddOtelLogger(configuration, resourceBuilder);

builder.Services.AddScoped<IInstrumentation>(_ => new Instrumentation(applicationName));

builder.Services.AddHttpClient<CustomerClientHandler>(client =>
{
    client.BaseAddress = new Uri(configuration.GetValue<string>("Services:Customer")!);
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    { ServerCertificateCustomValidationCallback = (_, _, _, _) => true });

builder.Services.AddGrpcClient<Product.ProductClient>((_, options) =>
{
    options.Address = new Uri(configuration.GetValue<string>("Services:Product")!);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/v1/customers", async (
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

app.MapGet("/v1/products", async (
        [FromServices] IInstrumentation instrumentation,
        [FromServices] Product.ProductClient client,
        CancellationToken cancellationToken) =>
    {
        using var activityMain =
            instrumentation.ActivitySource.StartActivity("Starting find products flow", ActivityKind.Server);

        var response = await client.FindAsync(new GetProductRequest(), cancellationToken: cancellationToken);

        return Results.Ok(new { TraceId = activityMain?.TraceId.ToString(), Content = response });
    })
    .WithName("Get Products")
    .WithOpenApi();

app.MapPost("/v1/products", async (
        [FromServices] IInstrumentation instrumentation,
        [FromServices] Product.ProductClient client,
        [FromBody] ProductRequest request,
        CancellationToken cancellationToken) =>
    {
        using var activityMain =
            instrumentation.ActivitySource.StartActivity("Starting create product flow", ActivityKind.Server);

        var response = await client.CreateAsync(new CreateProductRequest { Name = request.Name },
            cancellationToken: cancellationToken);

        return Results.Created($"/v1/products/{response.Id}",
            new { TraceId = activityMain?.TraceId.ToString(), Content = response });
    })
    .WithName("Create Product")
    .WithOpenApi();

app.Run();