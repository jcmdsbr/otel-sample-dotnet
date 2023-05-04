using Otel.Sample.WorkerService.Handlers.v1;

namespace Otel.Sample.WorkerService.Workers.v1;

public class ProducerWorker : BackgroundService
{
    private readonly ICustomerProducerHandler _producer;

    public ProducerWorker(ICustomerProducerHandler producer)
    {
        _producer = producer;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken).ConfigureAwait(false);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();

        while (!stoppingToken.IsCancellationRequested)
        {
            await _producer.StartAsync(stoppingToken);
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}