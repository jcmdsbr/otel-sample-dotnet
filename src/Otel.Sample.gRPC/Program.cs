using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using OpenTelemetry.Resources;
using Otel.Sample.gRPC.Repositories.v1;
using Otel.Sample.gRPC.Services;
using Otel.Sample.SharedKernel;
using Otel.Sample.SharedKernel.Diagnostics.v1;

var applicationName = "gRPCService";
var applicationVersion = "v1";
var applicationNamespace = "Otel.Sample";

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.AddGrpc();

var resourceBuilder = ResourceBuilder
    .CreateDefault()
    .AddService(applicationName, applicationNamespace, applicationVersion)
    .AddTelemetrySdk();

builder.Services.AddOtel(configuration, resourceBuilder, applicationName);
builder.Logging.AddOtelLogger(configuration, resourceBuilder);

builder.Services.AddScoped<IInstrumentation>(_ => new Instrumentation(applicationName));

var mongoClientSettings = MongoClientSettings.FromConnectionString(configuration.GetConnectionString("Mongo"));

builder.Services.AddSingleton(mongoClientSettings);

builder.Services.AddSingleton<IConventionPack>(new ConventionPack
{
    new EnumRepresentationConvention(BsonType.String),
    new IgnoreExtraElementsConvention(true),
    new IgnoreIfNullConvention(true)
});

builder.Services.AddSingleton<IMongoClient>(new MongoClient(mongoClientSettings));
builder.Services.AddScoped<IProductRepository, ProductRepository>();
var app = builder.Build();

app.MapGrpcService<ProductService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client.");

app.Run();