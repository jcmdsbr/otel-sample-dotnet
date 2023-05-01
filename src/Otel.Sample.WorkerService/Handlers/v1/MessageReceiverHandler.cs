using System.Diagnostics;
using System.Text;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using Otel.Sample.SharedKernel.Diagnostics.v1;
using Otel.Sample.SharedKernel.Helpers.v1;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Otel.Sample.WorkerService.Handlers.v1;

public interface IMessageReceiverHandler : IDisposable
{
    void StartConsumer();
}

public sealed class MessageReceiverHandler : IMessageReceiverHandler
{
    private static readonly TextMapPropagator Propagator = Propagators.DefaultTextMapPropagator;

    private readonly IModel _channel;
    private readonly IConnection _connection;
    private readonly IInstrumentation _instrumentation;
    private readonly ILogger<MessageReceiverHandler> _logger;

    private bool _disposedValue;

    public MessageReceiverHandler(
        ILogger<MessageReceiverHandler> logger,
        IConfiguration configuration,
        IInstrumentation instrumentation)
    {
        _logger = logger;
        _instrumentation = instrumentation;

        _connection = RabbitMqHelper.CreateConnection(configuration);
        _channel = RabbitMqHelper.CreateModelAndDeclareTestQueue(_connection);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public void StartConsumer()
    {
        RabbitMqHelper.StartConsumer(_channel, ReceiveMessage);
    }

    private void Dispose(bool disposing)
    {
        if (_disposedValue) return;

        if (disposing)
        {
            _channel.Dispose();
            _connection.Dispose();
        }

        _disposedValue = true;
    }

    private void ReceiveMessage(BasicDeliverEventArgs ea)
    {
        // Extract the PropagationContext of the upstream parent from the message headers.
        var parentContext = Propagator.Extract(default, ea.BasicProperties, ExtractTraceContextFromBasicProperties);
        Baggage.Current = parentContext.Baggage;

        // Start an activity with a name following the semantic convention of the OpenTelemetry messaging specification.
        // https://github.com/open-telemetry/opentelemetry-specification/blob/main/specification/trace/semantic_conventions/messaging.md#span-name
        var activityName = $"{ea.RoutingKey} receive";

        using var activity =
            _instrumentation.ActivitySource.StartActivity(activityName, ActivityKind.Consumer,
                parentContext.ActivityContext);
        try
        {
            var message = Encoding.UTF8.GetString(ea.Body.Span.ToArray());

            _logger.LogInformation($"Message received: [{message}]");

            activity?.SetTag("message", message);


            // These tags are added demonstrating the semantic conventions of the OpenTelemetry messaging specification
            // See:
            //   * https://github.com/open-telemetry/opentelemetry-specification/blob/main/specification/trace/semantic_conventions/messaging.md#messaging-attributes
            //   * https://github.com/open-telemetry/opentelemetry-specification/blob/main/specification/trace/semantic_conventions/messaging.md#rabbitmq

            activity?.SetTag("messaging.system", "rabbitmq");
            activity?.SetTag("messaging.destination_kind", "queue");
            activity?.SetTag("messaging.destination", RabbitMqHelper.DefaultExchangeName);
            activity?.SetTag("messaging.rabbitmq.routing_key", RabbitMqHelper.QueueName);

            // Simulate some work
            Thread.Sleep(1000);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Message processing failed.");
        }
    }

    private IEnumerable<string> ExtractTraceContextFromBasicProperties(IBasicProperties props, string key)
    {
        try
        {
            if (props.Headers.TryGetValue(key, out var value))
            {
                var bytes = value as byte[];
                return new[] { Encoding.UTF8.GetString(bytes!) };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract trace context.");
        }

        return Enumerable.Empty<string>();
    }

    ~MessageReceiverHandler()
    {
        Dispose(false);
    }
}