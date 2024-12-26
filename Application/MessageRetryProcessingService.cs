using Domain.Interfaces;
using Domain.Interfaces.Consumers;
using Infrastructure.RabbitMqUtil.Consumers;
using Microsoft.Extensions.Hosting;
using System.Threading;

namespace Application.Services
{
    public class MessageRetryProcessingService:BackgroundService
    {
        private readonly RetryTransactionConsumer _consumer;
        public MessageRetryProcessingService(RetryTransactionConsumer consumer)
        {
            _consumer = consumer;
        }
        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await base.StartAsync(cancellationToken);
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _consumer.ListenToQueueAsync(stoppingToken);
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
                await base.StopAsync(cancellationToken);
                await _consumer.CloseChannel();
        }
    }
}
