using OpenTelemetry.Resources;
using Otel.Sample.SharedKernel;
using Otel.Sample.SharedKernel.Diagnostics.v1;
using Otel.Sample.WorkerService;
using Otel.Sample.WorkerService.Handlers.v1;

var applicationName = "WorkService";
var applicationVersion = "v1";
var applicationNamespace = "Otel.Sample";


var resourceBuilder = ResourceBuilder
    .CreateDefault()
    .AddService(applicationName, applicationNamespace, applicationVersion)
    .AddTelemetrySdk();

var host = Host
    .CreateDefaultBuilder(args)
    .ConfigureLogging((host, logging) => { logging.AddOtelLogger(host.Configuration, resourceBuilder); })
    .ConfigureServices((host, services) =>
    {
        var configuration = host.Configuration;

        services.AddOtel(configuration, resourceBuilder, applicationName);
        services.AddCache(configuration);
        services.AddSingleton<IInstrumentation>(_ => new Instrumentation(applicationName));
        services.AddSingleton<IMessageReceiverHandler, MessageReceiverHandler>();
        services.AddHostedService<Worker>();
    })
    .Build();

host.Run();