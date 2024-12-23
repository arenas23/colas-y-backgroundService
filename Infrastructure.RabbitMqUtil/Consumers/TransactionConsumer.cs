using Domain.Entities.Dto;
using Domain.Entities.Helpers;
using Domain.Entities.Request;
using Domain.Interfaces.Consumers;
using Domain.Interfaces.Dian;
using Domain.Interfaces.Repositories;
using Infrastructure.RabbitMqUtil.Config;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.RateLimiting;
using System.Threading.Tasks;

namespace Infrastructure.RabbitMqUtil.Consumers
{
    public class TransactionConsumer : IConsumer
    {
        private SemaphoreSlim _rateLimiter;
        private readonly RabbitMqSettings _settings;
        private readonly ChannelManager _channelManager;
        private readonly IServiceProvider _serviceProvider;

        public TransactionConsumer(IOptions<RabbitMqSettings> settings, ChannelManager channelManager, IServiceProvider serviceProvider)
        {
            _rateLimiter = new(settings.Value.TransactionChannel.ConcurrentMessages, settings.Value.TransactionChannel.ConcurrentMessages); ;
            _settings = settings.Value;
            _channelManager = channelManager;
            _serviceProvider = serviceProvider;
        }
        public async Task ListenToQueueAsync(CancellationToken cancellationToken)
        {
            var consumer = new AsyncEventingBasicConsumer(_channelManager.TransactionChannel);
            var tasks = new List<Task>();
            Console.WriteLine("nueva config transaction" + _rateLimiter.CurrentCount);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                await _rateLimiter.WaitAsync(cancellationToken);
                tasks.Add(ProcessTransactionAsync(ea, cancellationToken));
            };

            await _channelManager.TransactionChannel.BasicConsumeAsync(_settings.TransactionChannel.Queue, false, consumer, cancellationToken);
            Console.WriteLine("Started listening to the TransactionQueue.");
        }

        private async Task ProcessTransactionAsync(BasicDeliverEventArgs ea, CancellationToken cancellationToken)
        {
            var message = ea.Body.ToArray();

            try
            {
                
                var transactionMessage = MessageHelper.DeserializeMessage<TransactionMessageDto>(message);
                var dianApi = _serviceProvider.GetRequiredService<IDianApi>(); 
                bool wasSent = await dianApi.SendToDian(transactionMessage.Payload, cancellationToken);
                if (!wasSent)
                {
                    await EnqueueToRetryQueue(transactionMessage, cancellationToken);
                    await _channelManager.TransactionChannel.BasicNackAsync(ea.DeliveryTag, false, false, cancellationToken);
                }
                else
                {
                    await _channelManager.TransactionChannel.BasicAckAsync(ea.DeliveryTag, false, cancellationToken);
                    Console.WriteLine("TRANSACCION PROCESADA");
                }
            }
            catch (Exception ex)
            {
                await _channelManager.RetryChannel.BasicPublishAsync(string.Empty, _settings.RetryChannel.Queue, true, _channelManager.MessageProperties, message,cancellationToken);
                await _channelManager.TransactionChannel.BasicNackAsync(ea.DeliveryTag, false, false, cancellationToken);
                Console.WriteLine("ERROR API DIAN: "+ ex.Message);
            }
            finally
            {
                _rateLimiter.Release();
            }


        }

        private async Task EnqueueToRetryQueue(TransactionMessageDto transaction, CancellationToken cancellationToken)
        {
            transaction.RetryCount += 1;
            var message = MessageHelper.SerializeMessage(transaction);
            await _channelManager.RetryChannel.BasicPublishAsync(string.Empty, _settings.RetryChannel.Queue,true, _channelManager.MessageProperties, message, cancellationToken);
        }

       

        public async Task CloseChannel()
        {
            await _channelManager.CloseTransactionChannel();
        }

        public async Task OpenChannel(ushort concurrentMessages, CancellationToken cancelationToken)
        {
            _rateLimiter = new(concurrentMessages, concurrentMessages);
            await _channelManager.InitializeTransactionChannel(concurrentMessages, cancelationToken);
        }

    }
}
