using Otel.Sample.SharedKernel;
using Otel.Sample.SharedKernel.Helpers.v1;
using Otel.Sample.WebService;
using Otel.Sample.WebService.Endpoints.v1;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

const string applicationName = "WebService";
var resourceBuilder = OTelHelper.Create(applicationName);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddOTel(configuration, resourceBuilder, applicationName);
builder.Logging.AddOTelLogger(configuration, resourceBuilder);

builder.Services.AddDistributedCacheWithRedis(configuration);
builder.Services.AddCustomServices(configuration);

var app = builder.Build().Init();
app = Customers.Register(app);

app.Run();