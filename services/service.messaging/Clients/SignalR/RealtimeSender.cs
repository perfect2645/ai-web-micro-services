using Logging;
using Microsoft.AspNetCore.SignalR;
using service.messaging.Hubs;
using service.shared.Models;
using Utils.Ioc;

namespace service.messaging.Clients.SignalR
{
    [Register(ServiceType = typeof(IRealtimeSender), Lifetime = Lifetime.Scoped)]
    public class RealtimeSender(IHubContext<SignalRHub> hubContext) : IRealtimeSender
    {

        private readonly IHubContext<SignalRHub> _hubContext = hubContext;

        public async Task SendToAllClientsAsync(DoraemonMessage message, CancellationToken cancellationToken = default)
        {
            try
            {
                Log4Logger.Logger.Info($"Start sending SignalR message to [all clients], message:[Source:{message.Source}, Id:{message.DoraemonItem.Id}]");

                await _hubContext.Clients.All   
                    .SendAsync("ReceiveRealTimeMessage", message, cancellationToken);

                Log4Logger.Logger.Info($"Successfully sent SignalR message to [all clients], message:[Source:{message.Source}, Id:{message.DoraemonItem.Id}]");
            }
            catch (OperationCanceledException)
            {

                Log4Logger.Logger.Warn($"A SignalR message has been cancelled. [all clients], message:[Source:{message.Source}, Id:{message.DoraemonItem.Id}]");
                throw;
            }
            catch (Exception ex)
            {
                Log4Logger.Logger.Error($"Failed to send message to [all clients], ", ex);
                throw new InvalidOperationException($"Failed to send message to [all clients]. Error:{ex.Message}", ex);
            }
        }

        public async Task SendToGroupAsync(string groupName, DoraemonMessage message, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(groupName);

            try
            {
                Log4Logger.Logger.Info($"Start sending SignalR message to [{groupName}], message:[Source:{message.Source}, Id:{message.DoraemonItem.Id}]");

                await _hubContext.Clients.Group(groupName)
                    .SendAsync("ReceiveRealTimeMessage", message, cancellationToken);

                Log4Logger.Logger.Info($"Successfully sent SignalR message to [{groupName}], message:[Source:{message.Source}, Id:{message.DoraemonItem.Id}]");
            }
            catch (OperationCanceledException)
            {

                Log4Logger.Logger.Warn($"A SignalR message has been cancelled. [{groupName}], message:[Source:{message.Source}, Id:{message.DoraemonItem.Id}]");
                throw;
            }
            catch (Exception ex)
            {
                Log4Logger.Logger.Error($"Failed to send message to group [{groupName}], ", ex);
                throw new InvalidOperationException($"Failed to send message to group [{groupName}]. Error:{ex.Message}", ex);
            }
        }
    }
}
