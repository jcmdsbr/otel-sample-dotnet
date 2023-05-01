using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Otel.Sample.SharedKernel.Helpers.v1;

public class MessageBrokerHelper
{
    private readonly ConnectionFactory _connectionFactory;

    public MessageBrokerHelper(ConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public static string DefaultExchangeName => "";
    public static string QueueName => "customer-created-queue";

    public IConnection CreateConnection()
    {
        return _connectionFactory.CreateConnection();
    }

    public IModel CreateModelAndDeclareTestQueue(IConnection connection)
    {
        var channel = connection.CreateModel();

        channel.QueueDeclare(QueueName, false, false, false, null);

        return channel;
    }

    public void StartConsumer(IModel channel, Action<BasicDeliverEventArgs> processMessage)
    {
        var consumer = new EventingBasicConsumer(channel);

        consumer.Received += (_, ea) => processMessage(ea);

        channel.BasicConsume(QueueName, true, consumer);
    }
}