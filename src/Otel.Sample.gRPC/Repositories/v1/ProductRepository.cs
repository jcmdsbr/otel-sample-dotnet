using System.Diagnostics;
using MongoDB.Driver;
using Otel.Sample.SharedKernel.Diagnostics.v1;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Otel.Sample.gRPC.Repositories.v1;

public interface IProductRepository
{
    Task AddAsync(Models.v1.Product product, CancellationToken cancellationToken);
    Task<IEnumerable<Models.v1.Product>> FindAsync(CancellationToken cancellationToken);
}

public class ProductRepository : IProductRepository
{
    private readonly IMongoCollection<Models.v1.Product> _collection;
    private readonly IInstrumentation _instrumentation;
    private readonly ILogger<ProductRepository> _logger;

    public ProductRepository(
        IInstrumentation instrumentation,
        IMongoClient client,
        ILogger<ProductRepository> logger)
    {
        _logger = logger;
        _instrumentation = instrumentation;
        _collection = client
            .GetDatabase("OTelSampleDb")
            .GetCollection<Models.v1.Product>(nameof(Models.v1.Product));
    }

    public async Task AddAsync(Models.v1.Product product, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Product to save: {productId}, {productName}", product.Id, product.Name);

        const string activityName = "Product.insert";
        using var activityCache = _instrumentation.ActivitySource.StartActivity(activityName, ActivityKind.Client);
        await _collection.InsertOneAsync(product, new InsertOneOptions(), cancellationToken);

        // The semantic conventions of the OpenTelemetry Mongodb specification
        activityCache?.SetTag("db.system", "mongodb");
        activityCache?.SetTag("db.user", "admin");
        activityCache?.SetTag("net.peer.name", "mongo");
        activityCache?.SetTag("net.peer.port", "27017");
        activityCache?.SetTag("net.transport", "IP.TCP");
        activityCache?.SetTag("db.name", "OTelSampleDb");
        activityCache?.SetTag("db.operation", "insert");
        activityCache?.SetTag("db.mongodb.collection", "Product");
        activityCache?.SetTag("db.redis.database_index", "0");
    }

    public async Task<IEnumerable<Models.v1.Product>> FindAsync(CancellationToken cancellationToken)
    {
        const string activityName = "Product.find";
        using var activityCache = _instrumentation.ActivitySource.StartActivity(activityName, ActivityKind.Client);

        var result = await _collection.Find(_ => true).ToListAsync(cancellationToken);

        // The semantic conventions of the OpenTelemetry Mongodb specification
        activityCache?.SetTag("db.system", "mongodb");
        activityCache?.SetTag("db.user", "admin");
        activityCache?.SetTag("net.peer.name", "mongo");
        activityCache?.SetTag("net.peer.port", "27017");
        activityCache?.SetTag("net.transport", "IP.TCP");
        activityCache?.SetTag("db.name", "OTelSampleDb");
        activityCache?.SetTag("db.operation", "insert");
        activityCache?.SetTag("db.mongodb.collection", "Product");
        activityCache?.SetTag("db.redis.database_index", "0");

        return result;
    }
}