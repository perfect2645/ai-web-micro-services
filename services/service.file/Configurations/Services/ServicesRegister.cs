using NetUtils.Aspnet.Configurations;
using service.file.Configurations.DomainSettings;

namespace service.file.Configurations.Services
{
    public static class ServicesRegister
    {
        public static void RegisterServices(this WebApplicationBuilder builder)
        {
            builder.RegisterAssembliesAutofac(new[]
            {
                typeof(Program).Assembly,
                //typeof(IShirtRepository).Assembly
            });
            builder.RegisterMiddlewares();
            builder.Configurations();
        }

        private static void RegisterMiddlewares(this WebApplicationBuilder builder)
        {
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            //builder.Services.AddApiVersioning()
        }

        private static void Configurations(this WebApplicationBuilder builder)
        {
            builder.Services.Configure<Settings>(builder.Configuration.GetSection(Constants.Settings));
        }
    }
}
