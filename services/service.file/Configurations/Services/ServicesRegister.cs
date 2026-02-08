using Microsoft.Extensions.FileProviders;
using NetUtils.Aspnet.Configurations;
using repository.doraemon.Repositories;
using service.file.Configurations.DomainSettings;

namespace service.file.Configurations.Services
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
                builder.Configurations();
                builder.RegisterMiddlewares();
            }

            private void RegisterMiddlewares()
            {
                // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
                builder.Services.AddOpenApi();

                RegisterStaticFileOptions(builder);
            }

            private void RegisterStaticFileOptions()
            {
                var staticFileConfig = builder.Configuration.GetSection(Constants.FileStorageSettings);
                if (staticFileConfig == null || staticFileConfig["LocalRootPath"] == null)
                {
                    throw new NullReferenceException("FileStorageSettings not found, please check your appsettings.json");
                }

                var staticFileOptions = new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(staticFileConfig["LocalRootPath"]!),
                    RequestPath = staticFileConfig["RequestPath"],
                    ServeUnknownFileTypes = true,
                };

                builder.Services.AddSingleton(staticFileOptions);
            }

            private void Configurations()
            {
                // build your customized configs under appsettings.json
                builder.Services.Configure<FileStorageSettings>(builder.Configuration.GetSection(Constants.FileStorageSettings));
            }
        }
    }
}
