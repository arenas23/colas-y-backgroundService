using Domain.Entities.Dto;
using Domain.Entities.Request;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Domain.Interfaces;
using Infrastructure.RabbitMqUtil.Config;
using Microsoft.Extensions.Options;
using Domain.Entities.Helpers;
using Domain.Interfaces.Repositories;


namespace Infrastructure.RabbitMqUtil
{
    public class MessageProcessor : IMessageProcessor
    {
        private readonly SemaphoreSlim _TransactionRateLimiter;
        private readonly SemaphoreSlim _retryRateLimiter;
        private const string URL_API = "http://localhost:5207/api/prueba";
        private readonly RabbitMqSettings _settings;
        private readonly ChannelManager _channelManager;
        private readonly IRabbitMqSettingsRepository _settingsRepository;

        public MessageProcessor(IOptions<RabbitMqSettings> settings, ChannelManager channelManager, IRabbitMqSettingsRepository settingsRepository) 
        {
            _settings = settings.Value;
            _channelManager = channelManager;
            _TransactionRateLimiter = new(settings.Value.TransactionChannel.ConcurrentMessages, settings.Value.TransactionChannel.ConcurrentMessages);
            _retryRateLimiter = new(settings.Value.RetryChannel.ConcurrentMessages, settings.Value.RetryChannel.ConcurrentMessages);
            _settingsRepository = settingsRepository;
        }
        public async Task Publish<T>(T message)
        {
            var body = MessageHelper.SerializeMessage(message);
            await _channelManager.TransactionChannel.BasicPublishAsync(string.Empty, _settings.TransactionChannel.Queue, body);
        }
        public async Task ListenToTransactionQueueAsync( CancellationToken cancellationToken)
        {
            var consumer = new AsyncEventingBasicConsumer(_channelManager.TransactionChannel);
            var tasks = new List<Task>();

            consumer.ReceivedAsync += async (model, ea) =>
            {
                await Task.Delay(1000);
                tasks.Add(ProcessTransactionAsync(ea));
            };

            await _channelManager.TransactionChannel.BasicConsumeAsync(_settings.TransactionChannel.Queue, false, consumer, cancellationToken);
            Console.WriteLine("Started listening to the queue.");
        }
        public async Task ListenToTransactionRetryQueueAsync(CancellationToken cancellationToken)
        {
            var consumer = new AsyncEventingBasicConsumer(_channelManager.RetryChannel);
            var tasks = new List<Task>();

            consumer.ReceivedAsync += async (model, ea) =>
            {

                //await _retryRateLimiter.WaitAsync();
                await Task.Delay(1000);
                tasks.Add(ExecuteTransactionRetry(ea));
   
                
            };

            await _channelManager.RetryChannel.BasicConsumeAsync(_settings.RetryChannel.Queue, false, consumer);
            Console.WriteLine("Started listening to the RetryQueue.");

        }
        private async Task ProcessTransactionAsync(BasicDeliverEventArgs ea)
        {
            try
            {
                var transactionMessage = MessageHelper.DeserializeMessage<TransactionMessageDto>(ea.Body);
                Console.WriteLine("Procesando mensaje...");
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
                await _channelManager.RetryChannel.BasicPublishAsync(string.Empty,_settings.RetryChannel.Queue, ea.Body);
                await _channelManager.TransactionChannel.BasicNackAsync(ea.DeliveryTag,false,false);
                Console.WriteLine("ERROR API DIAN: " + ex.Message);
            }
            //finally
            //{
            //    _TransactionRateLimiter.Release();
            //}


        }
        private async Task EnqueueToRetryQueue( TransactionMessageDto transaction)
        {
            transaction.RetryCount += 1;
            var message = MessageHelper.SerializeMessage(transaction);
            await _channelManager.RetryChannel.BasicPublishAsync(string.Empty, _settings.RetryChannel.Queue, message);
        }
        private async Task ExecuteTransactionRetry(BasicDeliverEventArgs ea)
        {
            
            var message = MessageHelper.DeserializeMessage<TransactionMessageDto>(ea.Body);
            message.RetryCount += 1;

            if (message.RetryCount <= 3)
            {
                await Task.Delay(5000);
            }
            else if (message.RetryCount <= 6)
            {
                await Task.Delay(10000);
            }
            else
            {
                await Task.Delay(15000);
            }
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
                await _channelManager.RetryChannel.BasicNackAsync(ea.DeliveryTag, false, false);
                await _channelManager.RetryChannel.BasicPublishAsync(string.Empty, _settings.RetryChannel.Queue, body);
                Console.WriteLine("error Api Reintento");

            }
            //finally
            //{
            //    _retryRateLimiter.Release();
            //}

           
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
        public async Task ChangeBasicQosAsync()
        {
            MessagesConcurrentDto settings = await _settingsRepository.RetrieveRabbitMqSettings();
            Console.WriteLine($"TransactionConcurrentMessages: {settings.TransactionConcurrentMessages}, RetryConcurrentMessages: {settings.RetryConcurrentMessages}");
            await _channelManager.TransactionChannel.BasicQosAsync(0, settings.TransactionConcurrentMessages, false);
            await _channelManager.RetryChannel.BasicQosAsync(0, settings.RetryConcurrentMessages, false);
        }
        public async Task CloseChannels()
        {
            await _channelManager.CloseChannels();
        }

        public async Task StartListeningAsync(CancellationToken cancellationToken)
        {
            var transactionListeningTask = ListenToTransactionQueueAsync(cancellationToken);
            var retryListeningTask = ListenToTransactionRetryQueueAsync(cancellationToken);

            // Espera que ambas tareas de escucha se completen
            await Task.WhenAll(transactionListeningTask, retryListeningTask);
        }
    }
}
