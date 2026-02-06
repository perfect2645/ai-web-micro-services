using Logging;
using NetUtils.Aspnet.Configurations;
using NetUtils.Aspnet.Configurations.Swagger;
using WebapiMq.Configurations;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.NetCoreLoggingSetup(Path.Combine("logs", builder.Environment.ApplicationName));
builder.Services.AddControllers();

builder.ConfigApiVersion();

// Add services to the container.
builder.RegisterCommonServices();
builder.RegisterServices();
builder.Services.AllowCorsExt();
builder.AddSwaggerGenExt($"{typeof(Program).Assembly.GetName().Name}.xml");

var app = builder.Build();

app.ConfigApp();

app.Run();
