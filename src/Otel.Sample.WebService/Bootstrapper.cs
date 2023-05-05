using Otel.Sample.SharedKernel.Helpers.v1;
using Otel.Sample.WebService.Handlers.v1;
using Otel.Sample.WebService.Repositories.v1;

namespace Otel.Sample.WebService;

public static class Bootstrapper
{
    public static IServiceCollection AddCustomServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<MessageBrokerHelper>();
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