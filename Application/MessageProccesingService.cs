﻿using Domain.Interfaces;
using Domain.Interfaces.Consumers;
using Infrastructure.RabbitMqUtil.Consumers;
using Microsoft.Extensions.Hosting;
using System.Threading;

namespace Application.Services
{
    public class MessageProccesingService : BackgroundService
    {
        private readonly TransactionConsumer _messageProcessor;
        public MessageProccesingService(TransactionConsumer messageProcessor)
        {
            _messageProcessor = messageProcessor;
        }
        public override async Task StartAsync(CancellationToken cancellationToken)
        {
         
            await base.StartAsync(cancellationToken);
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _messageProcessor.ListenToQueueAsync(stoppingToken);
            //await Task.Delay(Timeout.Infinite ,stoppingToken);
        }
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
            await _messageProcessor.CloseChannels();
        }

    }
}
