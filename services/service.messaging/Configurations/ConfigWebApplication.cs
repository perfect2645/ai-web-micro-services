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
                app.ConfigApp();
                // Mapping SignalR Hub endpoint (React client connection address: /realTimeHub)
                app.MapHub<SignalRHub>("/signalRHub");
            }
        }
    }
}
