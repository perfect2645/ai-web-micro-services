using Logging;
using service.file.Configurations.Services;
using Utils.Aspnet.Configurations;
using Utils.Aspnet.Configurations.Swagger;

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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerExt();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
