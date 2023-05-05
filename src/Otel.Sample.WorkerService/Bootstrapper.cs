using Otel.Sample.SharedKernel.Helpers.v1;
using Otel.Sample.WorkerService.Clients.v1;
using Otel.Sample.WorkerService.Handlers.v1;

namespace Otel.Sample.WorkerService;

public static class Bootstrapper
{
    public static IServiceCollection AddCustomServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<MessageBrokerHelper>();
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