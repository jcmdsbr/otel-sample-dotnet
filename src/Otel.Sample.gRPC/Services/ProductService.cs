using System.Diagnostics;
using Grpc.Core;
using Otel.Sample.gRPC.Repositories.v1;
using Otel.Sample.SharedKernel.Diagnostics.v1;

namespace Otel.Sample.gRPC.Services;

public class ProductService : Product.ProductBase
{
    private readonly IInstrumentation _instrumentation;
    private readonly ILogger<ProductService> _logger;
    private readonly IProductRepository _repository;

    public ProductService(IProductRepository repository, IInstrumentation instrumentation,
        ILogger<ProductService> logger)
    {
        _logger = logger;
        _repository = repository;
        _instrumentation = instrumentation;
    }

    public override async Task<CreateProductResponse> Create(CreateProductRequest request, ServerCallContext context)
    {
        using var activityMain =
            _instrumentation.ActivitySource.StartActivity("Starting create product process", ActivityKind.Server);

        var product = new Models.v1.Product(Guid.NewGuid(), request.Name);

        _logger.LogInformation("New product created: {productId}, {productName}", product.Id, product.Name);

        await _repository.AddAsync(product, context.CancellationToken);

        return new CreateProductResponse { Id = product.Id.ToString(), Name = product.Name };
    }

    public override async Task<ProoductsResponse> Find(GetProductRequest request, ServerCallContext context)
    {
        using var activityMain =
            _instrumentation.ActivitySource.StartActivity("Starting find products process", ActivityKind.Server);

        var result = await _repository.FindAsync(context.CancellationToken);

        _logger.LogInformation("Products found: {productCount}", result.Count());

        var response = new ProoductsResponse();

        response.Items.Add(result.Select(x => new ProductItem { Id = x.Id.ToString(), Name = x.Name }));

        return response;
    }
}