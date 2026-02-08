using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using NetUtils.Aspnet.Configurations;
using service.file.Configurations.DomainSettings;

namespace service.file.Configurations
{
    public static class ConfigWebApplication
    {
        extension(WebApplication app)
        {
            public void ConfigApplication()
            {
                var staticFileConfig = app.Services.GetRequiredService<IOptions<FileStorageSettings>>().Value;

                // enable static files folder (default wwwroot)
                app.UseStaticFiles();

                // static files for image output (React client access path: /img-output-remote)
                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(staticFileConfig.LocalRootPath),
                    RequestPath = staticFileConfig.RequestPath,
                    ServeUnknownFileTypes = true,
                });

                app.ConfigApp();
            }
        }
    }
}
