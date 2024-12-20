using Domain.Entities.Dto;
using Domain.Entities.Helpers;
using Domain.Entities.Request;
using Domain.Interfaces.Consumers;
using Domain.Interfaces.Repositories;
using Infrastructure.RabbitMqUtil.Config;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.RateLimiting;
using System.Threading.Tasks;

namespace Infrastructure.RabbitMqUtil.Consumers
{
    public class TransactionConsumer : IConsumer
    {
        private readonly SemaphoreSlim _rateLimiter;
        private readonly RabbitMqSettings _settings;
        private readonly ChannelManager _channelManager;
        private readonly IRabbitMqSettingsRepository _settingsRepository;
        private const string URL_API = "http://localhost:5207/api/prueba";

        public TransactionConsumer(IOptions<RabbitMqSettings> settings, ChannelManager channelManager, IRabbitMqSettingsRepository settingsRepository)
        {
            _rateLimiter = new(settings.Value.TransactionChannel.ConcurrentMessages, settings.Value.TransactionChannel.ConcurrentMessages); ;
            _settings = settings.Value;
            _channelManager = channelManager;
            _settingsRepository = settingsRepository;
        }
        public async Task ListenToQueueAsync(CancellationToken cancellationToken)
        {
            var consumer = new AsyncEventingBasicConsumer(_channelManager.TransactionChannel);
            var tasks = new List<Task>();

            consumer.ReceivedAsync += async (model, ea) =>
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
                await _rateLimiter.WaitAsync(cancellationToken);
                tasks.Add(ProcessTransactionAsync(ea));
            };

            await _channelManager.TransactionChannel.BasicConsumeAsync(_settings.TransactionChannel.Queue, false, consumer, cancellationToken);
            Console.WriteLine("Started listening to the TransactionQueue.");
        }

        private async Task ProcessTransactionAsync(BasicDeliverEventArgs ea)
        {
            var message = ea.Body.ToArray();

            try
            {
                
                var transactionMessage = MessageHelper.DeserializeMessage<TransactionMessageDto>(message);
                bool wasSent = await SendToDian(transactionMessage.Payload);
                if (!wasSent)
                {
                    await EnqueueToRetryQueue(transactionMessage);
                    await _channelManager.TransactionChannel.BasicNackAsync(ea.DeliveryTag, false, false);
                }
                else
                {
                    await _channelManager.TransactionChannel.BasicAckAsync(ea.DeliveryTag, false);
                    Console.WriteLine("TRANSACCION PROCESADA");
                }
            }
            catch (Exception ex)
            {
                await _channelManager.RetryChannel.BasicPublishAsync(string.Empty, _settings.RetryChannel.Queue, message);
                await _channelManager.TransactionChannel.BasicNackAsync(ea.DeliveryTag, false, false);
                Console.WriteLine("ERROR API DIAN: ");
            }
            finally
            {
                _rateLimiter.Release();
            }


        }

        private async Task EnqueueToRetryQueue(TransactionMessageDto transaction)
        {
            transaction.RetryCount += 1;
            var message = MessageHelper.SerializeMessage(transaction);
            await _channelManager.RetryChannel.BasicPublishAsync(string.Empty, _settings.RetryChannel.Queue, message);
        }

        public async Task CloseChannels()
        {
            
            await _channelManager.CloseTransactionChannel();
            
        }

        private async Task<bool> SendToDian(PeajeRequest transaction)
        {
            using var client = new HttpClient();
            using var request = new HttpRequestMessage(HttpMethod.Post, URL_API);
            var jsonBody = JsonSerializer.Serialize(transaction);
            request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            using var response = await client.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        public async Task Prendalo(CancellationToken cancelationToken)
        {
            await _channelManager.InitializeTransactionChannel(cancelationToken);
        }

    }
}
