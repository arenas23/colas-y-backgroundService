using Domain.Interfaces;
using Microsoft.Extensions.Hosting;

namespace Application.Services
{
    public class MessageProccesingService : BackgroundService
    {
        private readonly IMessageProcessor _messageProcessor;
        public MessageProccesingService(IMessageProcessor messageProcessor)
        {
            _messageProcessor = messageProcessor;
        }
        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await _messageProcessor.ListenToTransactionQueueAsync(cancellationToken);
            await base.StartAsync(cancellationToken);
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.CompletedTask;
        }
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
            await _messageProcessor.CloseChannels();
        }


        public async Task ChangeConcurrentMesagesAsync()
        {
            await _messageProcessor.ChangeBasicQosAsync();
        }

    }

    public class MessageRetryProcessingService:BackgroundService
    {
        private IMessageProcessor _messageProcessor;
        public MessageRetryProcessingService(IMessageProcessor messageProcessor)
        {
            _messageProcessor= messageProcessor;
        }


        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _messageProcessor.ListenToTransactionRetryQueueAsync(cancellationToken);
                await base.StartAsync(cancellationToken);
            }
            catch (Exception ex)
            {

                throw;
            }
            
            
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {

                throw;
            }
            
        }
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                await base.StopAsync(cancellationToken);
                await _messageProcessor.CloseChannels();
            }
            catch (Exception ex)
            {

                throw;
            }
            
        }
    }
}
