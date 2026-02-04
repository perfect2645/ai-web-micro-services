using Logging;
using NetUtils.Aspnet.Configurations;
using NetUtils.Aspnet.Configurations.Swagger;
using NetUtils.Aspnet.Filters;
using NetUtils.Repository.Configurations;
using repository.doraemon.Repositories;
using service.file.Configurations.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.NetCoreLoggingSetup(Path.Combine("logs", builder.Environment.ApplicationName));
builder.AddSqlServerContext<AppDbContext>("dev.doraemon");
builder.Services.AddControllers();

builder.ConfigApiVersion();

// Add services to the container.
builder.RegisterCommonServices();
builder.RegisterServices();
builder.Services.AllowCorsExt();
builder.AddSwaggerGenExt($"{typeof(Program).Assembly.GetName().Name}.xml", swaggerGenOptions =>
{
    // support file button in swagger
    swaggerGenOptions.OperationAsyncFilter<FileUploadOperationFilter>();
});

var app = builder.Build();

app.ConfigApp();

app.Run();
