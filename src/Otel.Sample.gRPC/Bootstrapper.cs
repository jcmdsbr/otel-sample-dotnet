using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using Otel.Sample.gRPC.Repositories.v1;

namespace Otel.Sample.gRPC;

public static class Bootstrapper
{
    public static IServiceCollection AddCustomServices(this IServiceCollection services, IConfiguration configuration)
    {
        var clientSettings = MongoClientSettings.FromConnectionString(configuration.GetConnectionString("Mongo"));

        services.AddSingleton(clientSettings);

        services.AddSingleton<IConventionPack>(new ConventionPack
        {
            new EnumRepresentationConvention(BsonType.String),
            new IgnoreExtraElementsConvention(true),
            new IgnoreIfNullConvention(true)
        });

        services.AddSingleton<IMongoClient>(new MongoClient(clientSettings));
        services.AddScoped<IProductRepository, ProductRepository>();

        return services;
    }
}