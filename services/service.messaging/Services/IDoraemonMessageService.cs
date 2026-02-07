using service.shared.Models;

namespace service.messaging.Services
{
    public interface IDoraemonMessageService
    {
        Task SendImageMessageAsync(DoraemonMessage message, CancellationToken ct = default);
        Task SendTopicMessageAsync(string topic, DoraemonMessage message, CancellationToken ct = default);
    }
}
