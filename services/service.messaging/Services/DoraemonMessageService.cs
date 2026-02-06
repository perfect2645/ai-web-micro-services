using service.messaging;
using service.messaging.Clients.RabbitMq.Producer;
using service.messaging.Model;
using service.messaging.Services;
using Utils.Ioc;
using WebapiMq.Model;

namespace WebapiMq.Services
{
    [Register(ServiceType = typeof(IDoraemonMessageService))]
    public class DoraemonMessageService : IDoraemonMessageService
    {
        private IRabbitMqProducer<DoraemonMessage> _queueModeMqProducer;
        private IRabbitMqProducer<DoraemonTopicMessage> _topicModeMqProducer;

        public DoraemonMessageService(
            [FromKeyedServices(MessagingConstants.Ioc_RabbitMq_QueueMode)] IRabbitMqProducer<DoraemonMessage> queueModeMqProducer,
            [FromKeyedServices(MessagingConstants.Ioc_RabbitMq_TopicMode)] IRabbitMqProducer<DoraemonTopicMessage> topicModeMqProducer)
        {
            _queueModeMqProducer = queueModeMqProducer;
            _topicModeMqProducer = topicModeMqProducer;
        }

        public async Task SendImageMessageAsync(string imagePath, string message, CancellationToken ct = default)
        {
            await _queueModeMqProducer.ProduceAsync(new DoraemonMessage(imagePath, message), ct);
        }

        public async Task SendTopicMessageAsync(string topic, string imagePath, string message, CancellationToken ct = default)
        {
            await _topicModeMqProducer.ProduceAsync(new DoraemonTopicMessage(topic, imagePath, message), ct);
        }
    }
}
