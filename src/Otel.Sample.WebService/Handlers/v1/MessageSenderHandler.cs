using System.Diagnostics;
using System.Text;
using System.Text.Json;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using Otel.Sample.SharedKernel.Diagnostics.v1;
using Otel.Sample.SharedKernel.Helpers.v1;
using Otel.Sample.SharedKernel.Models.v1;
using RabbitMQ.Client;

namespace Otel.Sample.WebService.Handlers.v1;

public interface IMessageSenderHandler : IDisposable
{
    Task SendMessageAsync(CustomerMessage customer, CancellationToken cancellationToken);
}

public sealed class MessageSenderHandler : IMessageSenderHandler
{
    private static readonly TextMapPropagator Propagator = Propagators.DefaultTextMapPropagator;

    private readonly IModel _channel;
    private readonly IConnection _connection;
    private readonly IInstrumentation _instrumentation;

    private bool _disposedValue;

    public MessageSenderHandler(IInstrumentation instrumentation, IConfiguration configuration)
    {
        _instrumentation = instrumentation;

        _connection = RabbitMqHelper.CreateConnection(configuration);
        _channel = RabbitMqHelper.CreateModelAndDeclareTestQueue(_connection);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public Task SendMessageAsync(CustomerMessage customer, CancellationToken cancellationToken)
    {
        return Task.Run(() =>
        {
            // Start an activity with a name following the semantic convention of the OpenTelemetry messaging specification.
            // https://github.com/open-telemetry/opentelemetry-specification/blob/main/specification/trace/semantic_conventions/messaging.md#span-name
            var activityName = $"{RabbitMqHelper.QueueName} send";

            using var activity = _instrumentation.ActivitySource.StartActivity(activityName, ActivityKind.Producer);
            var props = _channel.CreateBasicProperties();

            // Inject the ActivityContext into the message headers to propagate trace context to the receiving service.
            Propagator.Inject(new PropagationContext(Activity.Current!.Context, Baggage.Current), props,
                InjectTraceContextIntoBasicProperties);

            // These tags are added demonstrating the semantic conventions of the OpenTelemetry messaging specification
            // See:
            //   * https://github.com/open-telemetry/opentelemetry-specification/blob/main/specification/trace/semantic_conventions/messaging.md#messaging-attributes
            //   * https://github.com/open-telemetry/opentelemetry-specification/blob/main/specification/trace/semantic_conventions/messaging.md#rabbitmq

            activity?.SetTag("messaging.system", "rabbitmq");
            activity?.SetTag("messaging.destination_kind", "queue");
            activity?.SetTag("messaging.destination", RabbitMqHelper.DefaultExchangeName);
            activity?.SetTag("messaging.rabbitmq.routing_key", RabbitMqHelper.QueueName);

            _channel.BasicPublish(
                RabbitMqHelper.DefaultExchangeName,
                RabbitMqHelper.QueueName,
                props,
                Encoding.UTF8.GetBytes(JsonSerializer.Serialize(customer)));
        }, cancellationToken);
    }

    private void Dispose(bool disposing)
    {
        if (_disposedValue) return;

        if (disposing)
        {
            _channel.Dispose();
            _connection.Dispose();
            _instrumentation.Dispose();
        }

        _disposedValue = true;
    }

    private static void InjectTraceContextIntoBasicProperties(IBasicProperties props, string key, string value)
    {
        props.Headers ??= new Dictionary<string, object>();
        props.Headers[key] = value;
    }

    ~MessageSenderHandler()
    {
        Dispose(false);
    }
}