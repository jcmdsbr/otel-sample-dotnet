using Otel.Sample.WorkerService.Handlers.v1;

namespace Otel.Sample.WorkerService;

public class Worker : BackgroundService
{
    private readonly IMessageReceiverHandler _messageReceiverHandler;

    public Worker(IMessageReceiverHandler messageReceiverHandler)
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