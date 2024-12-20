
using Application.Services;
using Domain.Entities.Dto;
using Domain.Entities.Request;
using Domain.Interfaces;
using Domain.Interfaces.Publicer;
using Infrastructure.RabbitMqUtil;
using Infrastructure.RabbitMqUtil.Consumers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


namespace Services
{
    public class SenderService: ISenderService
    {
        private readonly Publisher _messagePublisher;
        private readonly IServiceProvider _serviceProvider;
        private readonly RetryTransactionConsumer _consumer;
        private Random _random;
        public SenderService(Publisher messagePublisher, IServiceProvider serviceProvider, RetryTransactionConsumer consumer)
        {
            _messagePublisher = messagePublisher;
            _serviceProvider = serviceProvider;
            _consumer = consumer;
            _random = new Random();
            
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

        public async Task ChangeConcurrency()
        {
            var messageRetryService = _serviceProvider.GetRequiredService<MessageRetryProcessingService>();

            // Crear un token de cancelación
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            // Detener el servicio de fondo MessageRetryProcessingService
            await messageRetryService.StopAsync(cancellationToken);

        }
        public async Task TurnOn()
        {
            var messageRetryService = _serviceProvider.GetRequiredService<MessageRetryProcessingService>();

            // Crear un token de cancelación
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;
            await _consumer.Prendalo(cancellationToken);

            // Detener el servicio de fondo MessageRetryProcessingService
            await messageRetryService.StartAsync(cancellationToken);

        }
    }
}
