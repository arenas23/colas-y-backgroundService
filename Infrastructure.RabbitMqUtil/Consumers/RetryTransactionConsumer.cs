using Domain.Entities.Dto;
using Domain.Entities.Helpers;
using Domain.Entities.Request;
using Domain.Interfaces.Consumers;
using Domain.Interfaces.Repositories;
using Infrastructure.RabbitMqUtil.Config;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Diagnostics;
using System.Text;
using System.Text.Json;


namespace Infrastructure.RabbitMqUtil.Consumers
{
    public class RetryTransactionConsumer: IConsumer
    {
        private readonly SemaphoreSlim _rateLimiter;
        private readonly RabbitMqSettings _settings;
        private readonly ChannelManager _channelManager;
        private readonly IRabbitMqSettingsRepository _settingsRepository;
        private const string URL_API = "http://localhost:5207/api/prueba";

        public RetryTransactionConsumer(IOptions<RabbitMqSettings> settings, ChannelManager channelManager, IRabbitMqSettingsRepository settingsRepository)
        {
            _rateLimiter = new(settings.Value.RetryChannel.ConcurrentMessages, settings.Value.RetryChannel.ConcurrentMessages); ;
            _settings = settings.Value;
            _channelManager = channelManager;
            _settingsRepository = settingsRepository;
        }

        public async Task ListenToQueueAsync(CancellationToken cancellationToken)
        {
            var consumer = new AsyncEventingBasicConsumer(_channelManager.RetryChannel);
            var tasks = new List<Task>();

            consumer.ReceivedAsync += async (model, ea) =>
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
                await _rateLimiter.WaitAsync(cancellationToken);
                tasks.Add(ExecuteTransactionRetry(ea));
            };

            await _channelManager.RetryChannel.BasicConsumeAsync(_settings.RetryChannel.Queue, false, consumer, cancellationToken);
            Console.WriteLine("Started listening to the RetryQueue.");
        }

        private async Task ExecuteTransactionRetry(BasicDeliverEventArgs ea)
        {
            var argument = ea.Body.ToArray();
            var message = MessageHelper.DeserializeMessage<TransactionMessageDto>(argument);
            message.RetryCount += 1;

            //if (message.RetryCount <= 3)
            //{
            //    await Task.Delay(5000);
            //}
            //else if (message.RetryCount <= 6)
            //{
            //    await Task.Delay(10000);
            //}
            //else
            //{
            //    await Task.Delay(15000);
            //}
            try
            {
                bool wasSent = await SendToDian(message.Payload);
                if (!wasSent)
                {
                    var body = MessageHelper.SerializeMessage(message);
                    await _channelManager.RetryChannel.BasicNackAsync(ea.DeliveryTag, false, false);
                    await _channelManager.RetryChannel.BasicPublishAsync(string.Empty, _settings.RetryChannel.Queue, body);
                    Console.WriteLine("Reintentar");
                }
                else
                {
                    await _channelManager.RetryChannel.BasicAckAsync(ea.DeliveryTag, false);
                    Console.WriteLine("REINTENTO PROCESADO");
                }
            }
            catch (Exception ex)
            {
                var body = MessageHelper.SerializeMessage(message);
                await _channelManager.RetryChannel.BasicPublishAsync(string.Empty, _settings.RetryChannel.Queue, body);
                await _channelManager.RetryChannel.BasicNackAsync(ea.DeliveryTag, false, false);
                Console.WriteLine("error Api Reintento");

            }
            finally
            {
                _rateLimiter.Release();
            }


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
        public async Task CloseChannels()
        {
            await _channelManager.CloseRetryTransactionChannel();
        }

        public async Task Prendalo(CancellationToken cancelationToken)
        {
            await _channelManager.InitializeRetryTransactionChannel(cancelationToken);
        }
    }
}
