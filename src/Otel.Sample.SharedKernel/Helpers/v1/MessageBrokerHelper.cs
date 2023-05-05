using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Otel.Sample.SharedKernel.Helpers.v1;

public class MessageBrokerHelper
{
    private readonly IConfiguration _configuration;

    public MessageBrokerHelper(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private ConnectionFactory? ConnectionFactory { get; set; }


    public static string DefaultExchangeName => "";
    public static string QueueName => "customer-created-queue";

    public IConnection CreateConnection()
    {
        try
        {
            if (ConnectionFactory != null) return ConnectionFactory.CreateConnection();

            ConnectionFactory = new ConnectionFactory
            {
                HostName = _configuration.GetValue<string>("Rabbit:HostName"),
                UserName = _configuration.GetValue<string>("Rabbit:UserName"),
                Password = _configuration.GetValue<string>("Rabbit:Password"),
                Port = 5672,
                RequestedConnectionTimeout = TimeSpan.FromMilliseconds(3000)
            };

            // TODO added to prevent the application from going up without rabbitmq being ready. Need to add healthcheck policies!!
            Thread.Sleep(TimeSpan.FromSeconds(10));

            return ConnectionFactory.CreateConnection();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public IModel CreateModelAndDeclareTestQueue(IConnection connection)
    {
        var channel = connection.CreateModel();

        channel.QueueDeclare(QueueName, false, false, false, null);

        return channel;
    }

    public void StartConsumer(IModel? channel, Action<BasicDeliverEventArgs> processMessage)
    {
        var consumer = new EventingBasicConsumer(channel);

        consumer.Received += (_, ea) => processMessage(ea);

        channel.BasicConsume(QueueName, true, consumer);
    }
}