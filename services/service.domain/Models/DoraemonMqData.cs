using repository.doraemon.Entities;

namespace service.domain.Models
{
    public record DoraemonMqData(DoraemonItem DoraemonItem, string? Source = null);
}
