using System.Diagnostics;
using MongoDB.Driver;
using Otel.Sample.gRPC.Models.v1;
using Otel.Sample.SharedKernel.Diagnostics.v1;

namespace Otel.Sample.gRPC.Repositories.v1;

public interface IProductRepository
{
    Task AddAsync(ProductEntity productEntity, CancellationToken cancellationToken);
    Task<IEnumerable<ProductEntity>> FindAsync(CancellationToken cancellationToken);
}

public class ProductRepository : IProductRepository
{
    private readonly IMongoCollection<ProductEntity> _collection;
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
            .GetCollection<ProductEntity>("ProductEntity");
    }

    public async Task AddAsync(ProductEntity productEntity, CancellationToken cancellationToken)
    {
        _logger.LogInformation("ProductEntity to save: {productId}, {productName}", productEntity.Id,
            productEntity.Name);

        const string activityName = "ProductEntity.insert";
        using var activityCache = _instrumentation.ActivitySource.StartActivity(activityName, ActivityKind.Client);
        await _collection.InsertOneAsync(productEntity, new InsertOneOptions(), cancellationToken);

        // The semantic conventions of the OpenTelemetry Mongodb specification
        activityCache?.SetTag("db.system", "mongodb");
        activityCache?.SetTag("db.user", "admin");
        activityCache?.SetTag("net.peer.name", "mongo");
        activityCache?.SetTag("net.peer.port", "27017");
        activityCache?.SetTag("net.transport", "IP.TCP");
        activityCache?.SetTag("db.name", "OTelSampleDb");
        activityCache?.SetTag("db.operation", "insert");
        activityCache?.SetTag("db.mongodb.collection", "ProductEntity");
    }

    public async Task<IEnumerable<ProductEntity>> FindAsync(CancellationToken cancellationToken)
    {
        const string activityName = "ProductEntity.find";
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
        activityCache?.SetTag("db.mongodb.collection", "ProductEntity");
        activityCache?.SetTag("db.redis.database_index", "0");

        return result;
    }
}