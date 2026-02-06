using Messaging.RabbitMq.Connections;
using Messaging.RabbitMq.Producer;
using service.domain.Models;
using service.messaging.Clients.Producer;
using Utils.Ioc;

namespace service.domain.Services
{
    [Register(ServiceType = typeof(MqProducerTopicMode), Key = DomainConstants.Ioc_RabbitMq_DoraemonData_TopicMode, Lifetime = Lifetime.Singleton)]
    public class DoraemonDataRabbitMqProducer : MqProducerTopicMode
    {
        public DoraemonDataRabbitMqProducer(IRabbitMqConnectionFactory connectionFactory) : base(connectionFactory)
        {
        }
    }
}
