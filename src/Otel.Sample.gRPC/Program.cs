using Otel.Sample.gRPC;
using Otel.Sample.gRPC.Services;
using Otel.Sample.SharedKernel;
using Otel.Sample.SharedKernel.Helpers.v1;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

const string applicationName = "gRPCService";
var resourceBuilder = OTelHelper.Create(applicationName);

builder.Services.AddGrpc();
builder.Services.AddOTel(configuration, resourceBuilder, applicationName);
builder.Logging.AddOTelLogger(configuration, resourceBuilder);
builder.Services.AddCustomServices(configuration);

var app = builder.Build();

app.MapGrpcService<ProductService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client.");

app.Run();