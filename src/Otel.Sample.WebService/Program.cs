using System.Diagnostics;
using OpenTelemetry.Resources;
using Otel.Sample.SharedKernel;
using Otel.Sample.SharedKernel.Models.v1;
using Otel.Sample.WebService.Diagnostics.v1;
using Otel.Sample.WebService.Handlers.v1;
using Otel.Sample.WebService.Repositories.v1;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

var applicationName = "WebService";
var applicationVersion = "v1";
var applicationNamespace = "Otel.Sample";

var resourceBuilder = ResourceBuilder.CreateDefault()
    .AddService(applicationName, applicationNamespace, applicationVersion).AddTelemetrySdk();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOtel(configuration, resourceBuilder, applicationName);
builder.Logging.AddOtelLogger(configuration, resourceBuilder);
builder.Services.AddCache(configuration);

builder.Services.AddScoped<Instrumentation>();
builder.Services.AddScoped<CustomerRepository>();
builder.Services.AddScoped<MessageSenderHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/v1/customers", async (
        CustomerRepository repository,
        MessageSenderHandler sender,
        Instrumentation instrumentation,
        Customer customer,
        CancellationToken cancellationToken) =>
    {
        using var activityMain = instrumentation.ActivitySource.StartActivity("Create a customer", ActivityKind.Server);

        var id = await repository.AddAsync(customer, cancellationToken);

        await sender.SendMessageAsync(customer, cancellationToken);

        return Results.Created($"/v1/customers/{id}", new
        {
            content = customer,
            traceId = activityMain?.TraceId.ToString()
        });
    })
    .WithName("Customers")
    .WithOpenApi();

app.Run();