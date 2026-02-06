using Messaging.RabbitMq;
using Messaging.RabbitMq.Connections;
using NetUtils.Aspnet.Configurations;
using repository.doraemon.Repositories;

namespace service.domain.Configurations.Services
{
    public static class ServicesRegister
    {
        extension(WebApplicationBuilder builder)
        {
            public void RegisterServices()
            {
                builder.RegisterAssembliesAutofac(
                [
                    typeof(Program).Assembly,
                    typeof(IFileRepository).Assembly,
                    typeof(IRabbitMqConnectionFactory).Assembly
                ]);
                builder.RegisterMiddlewares();
                builder.Configurations();
            }

            private void RegisterMiddlewares()
            {
                // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
                builder.Services.AddOpenApi();
            }

            private void Configurations()
            {
                builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection(DomainConstants.RabbitMqSettings));
            }
        }
    }
}
