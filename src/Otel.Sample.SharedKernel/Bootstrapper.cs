using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Instrumentation.GrpcNetClient;
using OpenTelemetry.Instrumentation.Http;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Otel.Sample.SharedKernel.Diagnostics.v1;

namespace Otel.Sample.SharedKernel;

public static class Bootstrapper
{
    public static ILoggingBuilder AddOTelLogger(this ILoggingBuilder builder, IConfiguration configuration,
        ResourceBuilder resourceBuilder)
    {
        var endpoint = configuration.GetValue<string>("OTelCol:Endpoint");

        ArgumentException.ThrowIfNullOrEmpty(endpoint);

        builder.ClearProviders();

        builder.AddOpenTelemetry(logProvider =>
        {
            logProvider.SetResourceBuilder(resourceBuilder);
            logProvider.AddOtlpExporter(options => options.Endpoint = new Uri(endpoint));
            logProvider.AddConsoleExporter();
        });

        return builder;
    }

    public static IServiceCollection AddOTel(
        this IServiceCollection services,
        IConfiguration configuration,
        ResourceBuilder resourceBuilder,
        string applicationName)
    {
        var endpoint = configuration.GetValue<string>("OTelCol:Endpoint");

        ArgumentException.ThrowIfNullOrEmpty(endpoint);

        services.AddOpenTelemetry().WithTracing(options =>
        {
            options
                .AddSource(applicationName)
                .SetResourceBuilder(resourceBuilder)
                .SetSampler(new AlwaysOnSampler())
                .AddHttpClientInstrumentation()
                .AddGrpcClientInstrumentation()
                .AddAspNetCoreInstrumentation();

            services.Configure<AspNetCoreInstrumentationOptions>(configuration.GetSection("AspNetCoreInstrumentation"));
            services.Configure<HttpClientInstrumentationOptions>(configuration.GetSection("HttpClientInstrumentation"));
            services.Configure<GrpcClientInstrumentationOptions>(configuration.GetSection("GrpcClientInstrumentation"));

            options.AddOtlpExporter(exporterOptions => { exporterOptions.Endpoint = new Uri(endpoint); });
            options.AddConsoleExporter();
        }).WithMetrics(options =>
        {
            options
                .AddMeter(applicationName)
                .SetResourceBuilder(resourceBuilder)
                .AddRuntimeInstrumentation()
                .AddHttpClientInstrumentation()
                .AddAspNetCoreInstrumentation();
            options.AddConsoleExporter();
            options.AddOtlpExporter(exporterOptions => { exporterOptions.Endpoint = new Uri(endpoint); });
        });

        services.AddTransient<IInstrumentation>(_ => new Instrumentation(applicationName));

        return services;
    }

    public static IServiceCollection AddDistributedCacheWithRedis(this IServiceCollection services,
        IConfiguration configuration)
    {
        return services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
            options.InstanceName = "SampleInstance";
        });
    }
}