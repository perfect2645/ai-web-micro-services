using NetUtils.Aspnet.Configurations;
using service.messaging;
using service.messaging.Configurations;

namespace WebapiMq.Configurations
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
                ]);
                builder.RegisterMiddlewares();
                builder.Configurations();
            }

            private void RegisterMiddlewares()
            {
                // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
                builder.Services.AddOpenApi();
                builder.Services.AddSignalR();
            }

            private void Configurations()
            {
                builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection(MessagingConstants.RabbitMqSettings));
                builder.Services.Configure<SignalRSettings>(builder.Configuration.GetSection(MessagingConstants.SignalRSettings));
            }
        }
    }
}
