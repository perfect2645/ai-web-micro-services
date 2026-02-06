using RabbitMQ.Client;

namespace service.messaging.Clients.RabbitMq.Connections
{
    public interface IDoraemonMqConnectionFactory
    {
        ConnectionFactory? ConnectionFactory { get; }
        ValueTask<IConnection> GetConnectionAsync();
    }
}
