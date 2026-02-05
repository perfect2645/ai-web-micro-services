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
                    typeof(IFileRepository).Assembly // your repository assemblies
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
                // build your customized configs under appsettings.json
                //builder.Services.Configure<FileStorageSettings>(builder.Configuration.GetSection(Constants.FileStorageSettings));
            }
        }
    }
}
