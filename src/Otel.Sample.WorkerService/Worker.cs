using Otel.Sample.WorkerService.Handlers.v1;

namespace Otel.Sample.WorkerService;

public class Worker : BackgroundService
{
    private readonly MessageReceiverHandler _messageReceiverHandler;

    public Worker(MessageReceiverHandler messageReceiverHandler)
    {
        _messageReceiverHandler = messageReceiverHandler;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken).ConfigureAwait(false);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();

        _messageReceiverHandler.StartConsumer();

        await Task.CompletedTask.ConfigureAwait(false);
    }
}