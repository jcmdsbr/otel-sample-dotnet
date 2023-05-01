using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Otel.Sample.SharedKernel.Helpers.v1;

public class RabbitMqHelper
{
    private readonly ConnectionFactory _connectionFactory;

    public RabbitMqHelper(IConfiguration configuration)
    {
        _connectionFactory = new ConnectionFactory
        {
            HostName = configuration.GetValue<string>("Rabbit:HostName"),
            UserName = configuration.GetValue<string>("Rabbit:UserName"),
            Password = configuration.GetValue<string>("Rabbit:Password"),
            Port = 5672,
            RequestedConnectionTimeout = TimeSpan.FromMilliseconds(3000)
        };
    }

    public string DefaultExchangeName { get; set; } = "";
    public string QueueName { get; set; } = "customer-created-queue";

    public IConnection CreateConnection()
    {
        return _connectionFactory.CreateConnection();
    }

    public IModel CreateModelAndDeclareTestQueue(IConnection connection)
    {
        var channel = connection.CreateModel();

        channel.QueueDeclare(
            QueueName,
            false,
            false,
            false,
            null);

        return channel;
    }

    public void StartConsumer(IModel channel, Action<BasicDeliverEventArgs> processMessage)
    {
        var consumer = new EventingBasicConsumer(channel);

        consumer.Received += (bc, ea) => processMessage(ea);

        channel.BasicConsume(QueueName, true, consumer);
    }
}