using Otel.Sample.Bff.Clients.v1;
using Otel.Sample.gRPC;

namespace Otel.Sample.Bff;

public static class Bootstrapper
{
    public static IServiceCollection AddCustomServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient<CustomerClientHandler>(client =>
        {
            client.BaseAddress = new Uri(configuration.GetValue<string>("Services:Customer")!);
        }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            { ServerCertificateCustomValidationCallback = (_, _, _, _) => true });

        services.AddGrpcClient<Product.ProductClient>((_, options) =>
        {
            options.Address = new Uri(configuration.GetValue<string>("Services:Product")!);
        });

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