using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Otel.Sample.SharedKernel.Helpers.v1;

public static class RabbitMqHelper
{
    private static ConnectionFactory? _connectionFactory;

    public static string DefaultExchangeName => "";
    public static string QueueName => "customer-created-queue";

    public static IConnection CreateConnection(IConfiguration configuration)
    {
        _connectionFactory ??= new ConnectionFactory
        {
            HostName = configuration.GetValue<string>("Rabbit:HostName"),
            UserName = configuration.GetValue<string>("Rabbit:UserName"),
            Password = configuration.GetValue<string>("Rabbit:Password"),
            Port = 5672,
            RequestedConnectionTimeout = TimeSpan.FromMilliseconds(3000)
        };

        return _connectionFactory.CreateConnection();
    }

    public static IModel CreateModelAndDeclareTestQueue(IConnection connection)
    {
        var channel = connection.CreateModel();

        channel.QueueDeclare(QueueName, false, false, false, null);

        return channel;
    }

    public static void StartConsumer(IModel channel, Action<BasicDeliverEventArgs> processMessage)
    {
        var consumer = new EventingBasicConsumer(channel);

        consumer.Received += (_, ea) => processMessage(ea);

        channel.BasicConsume(QueueName, true, consumer);
    }
}