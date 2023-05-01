using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Resources;
using Otel.Sample.SharedKernel;
using Otel.Sample.SharedKernel.Diagnostics.v1;
using Otel.Sample.SharedKernel.Helpers.v1;
using Otel.Sample.WebService.Handlers.v1;
using Otel.Sample.WebService.Models.v1;
using Otel.Sample.WebService.Repositories.v1;
using RabbitMQ.Client;

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
builder.Services.AddSingleton(_ => new MessageBrokerHelper(new ConnectionFactory
{
    HostName = configuration.GetValue<string>("Rabbit:HostName"),
    UserName = configuration.GetValue<string>("Rabbit:UserName"),
    Password = configuration.GetValue<string>("Rabbit:Password"),
    Port = 5672,
    RequestedConnectionTimeout = TimeSpan.FromMilliseconds(3000)
}));
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
        using var activityMain =
            instrumentation.ActivitySource.StartActivity("Starting create customer process", ActivityKind.Server);

        var (name, lastname) = request;
        var customer = new Customer(Guid.NewGuid(), name, lastname, DateTime.Now);

        await repository.AddAsync(customer, cancellationToken);

        await sender.SendMessageAsync(customer, cancellationToken);

        return Results.Created($"/v1/customers/{customer.Id}", new
        {
            content = request,
            traceId = activityMain?.TraceId.ToString()
        });
    })
    .WithName("Customers")
    .WithOpenApi();

app.Run();