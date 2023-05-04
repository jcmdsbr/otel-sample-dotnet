using Otel.Sample.SharedKernel;
using Otel.Sample.SharedKernel.Helpers.v1;
using Otel.Sample.WorkerService;
using Otel.Sample.WorkerService.Workers.v1;

var applicationName = "WorkerService";
var resourceBuilder = OTelHelper.Create(applicationName);

var host = Host
    .CreateDefaultBuilder(args)
    .ConfigureLogging((host, logging) => { logging.AddOTelLogger(host.Configuration, resourceBuilder); })
    .ConfigureServices((host, services) =>
    {
        var configuration = host.Configuration;

        services.AddOTel(configuration, resourceBuilder, applicationName);
        services.AddDistributedCacheWithRedis(configuration);
        services.AddCustomServices(configuration);

        services.AddHostedService<NotificationWorker>();
        services.AddHostedService<ProducerWorker>();
    })
    .Build();

host.Run();