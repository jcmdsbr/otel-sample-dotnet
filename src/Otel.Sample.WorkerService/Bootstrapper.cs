using Otel.Sample.SharedKernel.Helpers.v1;
using Otel.Sample.WorkerService.Clients.v1;
using Otel.Sample.WorkerService.Handlers.v1;
using RabbitMQ.Client;

namespace Otel.Sample.WorkerService;

public static class Bootstrapper
{
    public static IServiceCollection AddCustomServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(_ => new MessageBrokerHelper(new ConnectionFactory
        {
            HostName = configuration.GetValue<string>("Rabbit:HostName"),
            UserName = configuration.GetValue<string>("Rabbit:UserName"),
            Password = configuration.GetValue<string>("Rabbit:Password"),
            Port = 5672,
            RequestedConnectionTimeout = TimeSpan.FromMilliseconds(3000)
        }));

        services.AddSingleton<IMessageReceiverHandler, MessageReceiverHandler>();
        services.AddSingleton<ICustomerProducerHandler, CustomerProducerHandler>();
        services.AddHttpClient<CustomerClientHandler>(client =>
        {
            client.BaseAddress = new Uri(configuration.GetValue<string>("Services:Customer")!);
        }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            { ServerCertificateCustomValidationCallback = (_, _, _, _) => true });

        return services;
    }
}