using NetUtils.Aspnet.Configurations;
using repository.doraemon.Repositories;
using service.file.Configurations.DomainSettings;

namespace service.file.Configurations.Services
{
    public static class ServicesRegister
    {
        public static void RegisterServices(this WebApplicationBuilder builder)
        {
            builder.RegisterAssembliesAutofac(
            [
                typeof(Program).Assembly,
                typeof(IFileRepository).Assembly
            ]);
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
            builder.Services.Configure<FileStorageSettings>(builder.Configuration.GetSection(Constants.FileStorageSettings));
        }
    }
}
