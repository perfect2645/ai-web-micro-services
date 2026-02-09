using Messaging.RabbitMq.Connections;
using Messaging.RabbitMq.Producer;
using service.domain.Models;
using service.messaging.Clients.Producer;
using service.shared.Models;
using Utils.Ioc;

namespace service.domain.Services
{
    [Register(ServiceType = typeof(MqProducerTopicMode<DoraemonMessage>), Key = DomainConstants.Ioc_RabbitMq_DoraemonData_TopicMode, Lifetime = Lifetime.Singleton)]
    public class DoraemonDataRabbitMqProducer : MqProducerTopicMode<DoraemonMessage>
    {
        public DoraemonDataRabbitMqProducer(IRabbitMqConnectionFactory connectionFactory) : base(connectionFactory)
        {
        }
    }
}
