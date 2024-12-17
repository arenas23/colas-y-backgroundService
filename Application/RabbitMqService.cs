using Domain.Interfaces.RabbitMqUtil;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace Application.Services
{
    public class RabbitMqService : BackgroundService
    {
        private readonly IRabbitMqUtil _rabbitMqUtil;
        private IConnection? _connection;
        private IChannel? _transactionChannel;  
        private IChannel? _retryChannel;       
        private const string TRANSACTION_QUEUE = "transaction-queue";
        private const string RETRY_QUEUE = "transaction-retry-queue";
        private const int MAX_CONCURRENT_MESSAGES = 5;
        public RabbitMqService(IRabbitMqUtil rabbitMqUtil)
        {
            _rabbitMqUtil = rabbitMqUtil;
        }
        public override async Task StartAsync(CancellationToken cancellationToken)
        {
           
            await CreateChannels(cancellationToken);
            await _rabbitMqUtil.ListenToTransactionQueueAsync(_transactionChannel, TRANSACTION_QUEUE, RETRY_QUEUE, cancellationToken);
            await _rabbitMqUtil.ListenToTransactionRetryQueueAsync(_retryChannel, RETRY_QUEUE, cancellationToken);
            await base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.CompletedTask;
        }
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
            if (_transactionChannel != null) await _transactionChannel.CloseAsync(cancellationToken);
            if (_retryChannel != null) await _retryChannel.CloseAsync(cancellationToken);
            if (_connection != null) await _connection.CloseAsync(cancellationToken);
        }

        private async Task CreateChannels(CancellationToken cancellationToken)
        {
            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest"
            };

            _connection = await factory.CreateConnectionAsync(cancellationToken);
           
            _transactionChannel = await _connection.CreateChannelAsync(null, cancellationToken);
            await _transactionChannel.QueueDeclareAsync(TRANSACTION_QUEUE, false, false, false, null,false, cancellationToken);
            await _transactionChannel.BasicQosAsync(0, MAX_CONCURRENT_MESSAGES, false, cancellationToken); 

            
            _retryChannel = await _connection.CreateChannelAsync(null, cancellationToken);
            await _retryChannel.QueueDeclareAsync(RETRY_QUEUE, false, false, false, null, false, cancellationToken);
            await _retryChannel.BasicQosAsync(0, MAX_CONCURRENT_MESSAGES, false, cancellationToken); 
        }

    }
}
