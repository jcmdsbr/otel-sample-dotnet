using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Resources;
using Otel.Sample.SharedKernel;
using Otel.Sample.SharedKernel.Diagnostics.v1;
using Otel.Sample.SharedKernel.Models.v1;
using Otel.Sample.WebService.Handlers.v1;
using Otel.Sample.WebService.Repositories.v1;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

var applicationName = "WebService";
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
builder.Services.AddCache(configuration);

builder.Services.AddScoped<IInstrumentation>(_ => new Instrumentation(applicationName));
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IMessageSenderHandler, MessageSenderHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/v1/customers", async (
        [FromServices] ICustomerRepository repository,
        [FromServices] IMessageSenderHandler sender,
        [FromServices] IInstrumentation instrumentation,
        [FromBody] CustomerRequest request,
        CancellationToken cancellationToken) =>
    {
        using var activityMain = instrumentation.ActivitySource.StartActivity("Create a customer", ActivityKind.Server);

        var id = await repository.AddAsync(request, cancellationToken);

        var message = new CustomerMessage(id, request.Name, request.LastName, DateTime.Now);

        await sender.SendMessageAsync(message, cancellationToken);

        return Results.Created($"/v1/customers/{id}", new
        {
            content = request,
            traceId = activityMain?.TraceId.ToString()
        });
    })
    .WithName("Customers")
    .WithOpenApi();

app.Run();