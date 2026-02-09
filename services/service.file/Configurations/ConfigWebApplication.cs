using Microsoft.Extensions.FileProviders;
using NetUtils.Aspnet.Configurations;

namespace service.file.Configurations
{
    public static class ConfigWebApplication
    {
        extension(WebApplication app)
        {
            public void ConfigApplication()
            {
                var staticFileConfig = app.Services.GetRequiredService<StaticFileOptions>();

                // enable static files folder (default wwwroot)
                app.UseStaticFiles();

                // static files for image output (React client access path: /img-output-remote)
                app.UseStaticFiles(staticFileConfig);

                app.ConfigApp();
            }
        }
    }
}
