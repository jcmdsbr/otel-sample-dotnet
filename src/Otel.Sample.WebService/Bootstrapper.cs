using Otel.Sample.SharedKernel.Helpers.v1;
using Otel.Sample.WebService.Handlers.v1;
using Otel.Sample.WebService.Repositories.v1;
using RabbitMQ.Client;

namespace Otel.Sample.WebService;

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

        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IMessageSenderHandler, MessageSenderHandler>();

        return services;
    }

    public static WebApplication Init(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        return app;
    }
}