using Messaging.RabbitMq.Models;
using repository.doraemon.Entities;

namespace service.domain.Models
{
    public record DoraemonMqData(string Topic, DoraemonItem DoraemonItem, string? Source = null) : ITopicPayload;
}
