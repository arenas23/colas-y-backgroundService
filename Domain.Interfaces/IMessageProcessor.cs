using RabbitMQ.Client;

namespace Domain.Interfaces
{
    public interface IMessageProcessor
    {
        Task CloseChannels();
        Task Publish<T>(T message);
        Task ListenToTransactionQueueAsync( CancellationToken cancellationToken);
        Task ListenToTransactionRetryQueueAsync(CancellationToken cancellationToken);
        Task ChangeBasicQosAsync();

        Task StartListeningAsync(CancellationToken cancellationToken);
    }
}