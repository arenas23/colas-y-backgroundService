using RabbitMQ.Client;

namespace Domain.Interfaces.RabbitMqUtil
{
    public interface IRabbitMqUtil
    {
        Task Publish<T>(IChannel channel, string queue, T message);
        Task ListenToTransactionQueueAsync(IChannel channel, string transactionQueue, string retryQueue, CancellationToken cancellationToken);
        Task ListenToTransactionRetryQueueAsync(IChannel channel, string queue, CancellationToken cancellationToken);
    }
}