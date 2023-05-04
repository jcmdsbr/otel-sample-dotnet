using Otel.Sample.Bff;
using Otel.Sample.Bff.Endpoints.v1;
using Otel.Sample.SharedKernel;
using Otel.Sample.SharedKernel.Helpers.v1;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

const string applicationName = "Bff";
var resourceBuilder = OTelHelper.Create(applicationName);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOTel(configuration, resourceBuilder, applicationName);
builder.Logging.AddOTelLogger(configuration, resourceBuilder);
builder.Services.AddCustomServices(configuration);


var app = builder.Build().Init();

app = Customers.Register(app);
app = Products.Register(app);

app.Run();