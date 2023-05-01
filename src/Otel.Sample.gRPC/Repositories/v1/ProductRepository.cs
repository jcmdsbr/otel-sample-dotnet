using System.Diagnostics;
using MongoDB.Driver;
using Otel.Sample.SharedKernel.Diagnostics.v1;

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
            .GetDatabase("OtelSampleDb")
            .GetCollection<Models.v1.Product>(nameof(Models.v1.Product));
    }

    public async Task AddAsync(Models.v1.Product product, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Product to save: {productId}, {productName}", product.Id, product.Name);
        using var activityCache =
            _instrumentation.ActivitySource.StartActivity("Saving product in mongo", ActivityKind.Client);
        await _collection.InsertOneAsync(product, new InsertOneOptions(), cancellationToken);
    }

    public async Task<IEnumerable<Models.v1.Product>> FindAsync(CancellationToken cancellationToken)
    {
        return await _collection.Find(_ => true).ToListAsync(cancellationToken);
    }
}