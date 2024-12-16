using RabbitMQ.Client;

namespace Domain.Interfaces.RabbitMqUtil
{
    public interface IRabbitMqUtil
    {
        Task PublishMessageQueue(string routingKey, byte[] body);
        //Task ListenMessageQueue(IChannel channel, string routingkey, CancellationToken cancellationToken);
        Task ListenToQueueAsync(IChannel channel, CancellationToken cancellationToken);
    }
}