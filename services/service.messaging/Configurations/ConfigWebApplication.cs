using Microsoft.Extensions.FileProviders;
using NetUtils.Aspnet.Configurations;
using service.messaging.Hubs;

namespace service.messaging.Configurations
{
    public static class ConfigWebApplication
    {
        extension(WebApplication app)
        {
            public void ConfigApplication()
            {
                // enable static files folder (default wwwroot)
                app.UseStaticFiles();

                // static files for image output (React client access path: /img-output-remote)
                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(@"F:\dev-storage\image-ai"),
                    RequestPath = "/imgs-remote"
                });

                app.ConfigApp();
                // Mapping SignalR Hub endpoint (React client connection address: /realTimeHub)
                app.MapHub<SignalRHub>("/signalRHub");
            }
        }
    }
}
