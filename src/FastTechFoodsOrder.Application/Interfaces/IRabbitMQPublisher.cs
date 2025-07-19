namespace FastTechFoodsOrder.Application.Interfaces
{
    public interface IRabbitMQPublisher
    {
        Task PublishAsync<T>(T message, string exchange, string routingKey) where T : class;
        Task PublishAsync<T>(T message, string queueName) where T : class;
    }
}
