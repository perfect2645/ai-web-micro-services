using service.messaging;
using service.messaging.Clients.RabbitMq.Producer;
using service.messaging.Services;
using service.shared.Models;
using Utils.Ioc;

namespace WebapiMq.Services
{
    [Register(ServiceType = typeof(IDoraemonMessageService))]
    public class DoraemonMessageService : IDoraemonMessageService
    {
        private IRabbitMqProducer<DoraemonMessage> _queueModeMqProducer;
        private IRabbitMqProducer<DoraemonMessage> _topicModeMqProducer;

        public DoraemonMessageService(
            [FromKeyedServices(MessagingConstants.Ioc_RabbitMq_QueueMode)] IRabbitMqProducer<DoraemonMessage> queueModeMqProducer,
            [FromKeyedServices(MessagingConstants.Ioc_RabbitMq_TopicMode)] IRabbitMqProducer<DoraemonMessage> topicModeMqProducer)
        {
            _queueModeMqProducer = queueModeMqProducer;
            _topicModeMqProducer = topicModeMqProducer;
        }

        public async Task SendImageMessageAsync(DoraemonMessage message, CancellationToken ct = default)
        {
            await _queueModeMqProducer.ProduceAsync(message, ct);
        }

        public async Task SendTopicMessageAsync(string topic, DoraemonMessage message, CancellationToken ct = default)
        {
            await _topicModeMqProducer.ProduceAsync(message, ct);
        }
    }
}
