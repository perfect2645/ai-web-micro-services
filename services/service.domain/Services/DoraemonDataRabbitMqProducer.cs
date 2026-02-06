using Messaging.RabbitMq.Producer;
using service.domain.Models;

namespace service.domain.Services
{
    public class DoraemonDataRabbitMqProducer : IRabbitMqProducer<DoraemonMqData>
    {
        public Task ProduceAsync(DoraemonMqData messagePayload, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}
