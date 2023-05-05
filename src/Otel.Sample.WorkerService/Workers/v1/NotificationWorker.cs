using Otel.Sample.WorkerService.Handlers.v1;

namespace Otel.Sample.WorkerService.Workers.v1;

public class NotificationWorker : BackgroundService
{
    private readonly IMessageReceiverHandler _messageReceiverHandler;

    public NotificationWorker(IMessageReceiverHandler messageReceiverHandler)
    {
        _messageReceiverHandler = messageReceiverHandler;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken).ConfigureAwait(false);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            await _messageReceiverHandler.StartAsync(stoppingToken);
        }
    }
}