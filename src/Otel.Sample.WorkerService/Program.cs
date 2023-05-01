using OpenTelemetry.Resources;
using Otel.Sample.SharedKernel;
using Otel.Sample.SharedKernel.Diagnostics.v1;
using Otel.Sample.SharedKernel.Helpers.v1;
using Otel.Sample.WorkerService;
using Otel.Sample.WorkerService.Handlers.v1;
using RabbitMQ.Client;

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
        services.AddSingleton(_ => new MessageBrokerHelper(new ConnectionFactory
        {
            HostName = configuration.GetValue<string>("Rabbit:HostName"),
            UserName = configuration.GetValue<string>("Rabbit:UserName"),
            Password = configuration.GetValue<string>("Rabbit:Password"),
            Port = 5672,
            RequestedConnectionTimeout = TimeSpan.FromMilliseconds(3000)
        }));
        services.AddSingleton<IInstrumentation>(_ => new Instrumentation(applicationName));
        services.AddSingleton<IMessageReceiverHandler, MessageReceiverHandler>();
        services.AddHostedService<Worker>();
    })
    .Build();

host.Run();