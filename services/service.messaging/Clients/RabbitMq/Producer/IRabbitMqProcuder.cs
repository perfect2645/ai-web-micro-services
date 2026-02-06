namespace service.messaging.Clients.RabbitMq.Producer
{
    public interface IRabbitMqProducer<Payload> where Payload : class
    {
        Task ProduceAsync(Payload messagePayload, CancellationToken ct = default);
    }
}
