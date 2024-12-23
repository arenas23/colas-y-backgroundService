
using Application.Services;
using Domain.Entities.Dto;
using Domain.Entities.Request;
using Domain.Interfaces.Publicer;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services;
using Infrastructure.RabbitMqUtil;
using Infrastructure.RabbitMqUtil.Consumers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


namespace Services
{
    public class QueueService: IQueueService
    {
        private readonly Publisher _messagePublisher;
        private readonly IServiceProvider _serviceProvider;
        private readonly RetryTransactionConsumer _retryConsumer;
        private readonly TransactionConsumer _transactionConsumer;
        private readonly IRabbitMqSettingsRepository _rabbitMqSettingsRepository;
        private Random _random;
        public QueueService(Publisher messagePublisher, IServiceProvider serviceProvider, RetryTransactionConsumer retryConsumer, TransactionConsumer transactionConsumer, IRabbitMqSettingsRepository rabbitMqSettingsRepository)
        {
            _random = new Random();
            _messagePublisher = messagePublisher;
            _serviceProvider = serviceProvider;
            _retryConsumer = retryConsumer;
            _transactionConsumer = transactionConsumer;
            _rabbitMqSettingsRepository = rabbitMqSettingsRepository;
        }

        public async Task SendMessageToQueue(PeajeRequest request)
        {

            var tasks = new List<Task>();

            for (var i = 0; i < 100; i++)
            {
                var message = new TransactionMessageDto
                {
                    Payload = request,
                    FirtAttemptTime = DateTime.UtcNow,
                    RetryCount = 0
                };

                tasks.Add(_messagePublisher.Publish(message));
            }

            await Task.WhenAll(tasks);

            Console.WriteLine("enviado");
        }

        public async Task StopConsumers()
        {
            var retryService = _serviceProvider.GetRequiredService<MessageRetryProcessingService>();
            var transactionService = _serviceProvider.GetRequiredService<MessageProccesingService>();

            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            await retryService.StopAsync(cancellationToken);
            await transactionService.StopAsync(cancellationToken);

        }
        public async Task TurnOnConsumers()
        {
            var retryService = _serviceProvider.GetRequiredService<MessageRetryProcessingService>();
            var transactionService = _serviceProvider.GetRequiredService<MessageProccesingService>();

            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;
            var settings = await _rabbitMqSettingsRepository.RetrieveRabbitMqSettings();
            await _retryConsumer.OpenChannel(settings.RetryConcurrentMessages, cancellationToken);
            await _transactionConsumer.OpenChannel(settings.TransactionConcurrentMessages, cancellationToken);

            await retryService.StartAsync(cancellationToken);
            await transactionService.StartAsync(cancellationToken);

        }
    }
}
