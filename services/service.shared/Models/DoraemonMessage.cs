using Messaging.RabbitMq.Models;
using repository.doraemon.Entities;

namespace service.shared.Models
{
    public record DoraemonMessage(string Topic, DoraemonItem DoraemonItem, string? Source = null) : ITopicPayload;
}
