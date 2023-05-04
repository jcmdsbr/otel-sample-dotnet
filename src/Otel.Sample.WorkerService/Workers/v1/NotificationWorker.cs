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

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();

        _messageReceiverHandler.StartConsumer();

        return Task.CompletedTask;
    }
}