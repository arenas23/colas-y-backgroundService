using Domain.Entities.Dto;
using Domain.Entities.Request;
using Domain.Interfaces.RabbitMqUtil;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Net.Http;
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using System.Threading.RateLimiting;
using System.Threading.Tasks;
using System.Net.Http.Headers;


namespace Infrastructure.RabbitMqUtil
{
    public class RabbitMqUtil : IRabbitMqUtil
    {
        private readonly SemaphoreSlim _TransactionRateLimiter;
        private readonly SemaphoreSlim _retryRateLimiter;
        private const int MaxConcurrentTasks = 5;
        private const int MaxConcurrentRetrys = 5;
        private const string URL_API = "http://localhost:5207/api/prueba";
        public RabbitMqUtil() 
        {
            _TransactionRateLimiter = new(MaxConcurrentTasks, MaxConcurrentTasks);
            _retryRateLimiter = new(MaxConcurrentRetrys,MaxConcurrentRetrys);
        }

        public async Task Publish<T>(IChannel channel, string queue,T message)
        {
            var body = SerializeMessage(message);
            await channel.BasicPublishAsync(string.Empty, queue, body);
        }
        public async Task ListenToTransactionQueueAsync(IChannel channel, string transactionQueue, string retryQueue, CancellationToken cancellationToken)
        {
            var consumer = new AsyncEventingBasicConsumer(channel);
            var tasks = new List<Task>();

            consumer.ReceivedAsync += async (model, ea) =>
            {
                await _TransactionRateLimiter.WaitAsync();
                tasks.Add(ProcessTransactionAsync(ea, channel, retryQueue));
            };

            await channel.BasicConsumeAsync(transactionQueue, false, consumer);
            Console.WriteLine("Started listening to the queue.");
        }
        public async Task ListenToTransactionRetryQueueAsync(IChannel channel, string queue, CancellationToken cancellationToken)
        {
            var consumer = new AsyncEventingBasicConsumer(channel);
            var tasks = new List<Task>();

            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    await _retryRateLimiter.WaitAsync();
                    tasks.Add(ExecuteTransactionRetry(ea, channel, queue));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing Retry Transaction: {ex.Message}");
                    await channel.BasicNackAsync(ea.DeliveryTag, false, true);
                }
            };

            await channel.BasicConsumeAsync(queue, false, consumer);
            Console.WriteLine("Started listening to the RetryQueue.");

        }
        private async Task ProcessTransactionAsync(BasicDeliverEventArgs ea, IChannel channel, string retryQueue)
        {
            try
            {
                var transactionMessage = DeserializeMessage<TransactionMessageDto>(ea.Body);
                Console.WriteLine("Procesando mensaje...");
                bool wasSent = await SendToDian(transactionMessage.Payload);
                if (!wasSent)
                {
                    await EnqueueToRetryQueue(retryQueue, channel, transactionMessage);
                    await channel.BasicNackAsync(ea.DeliveryTag, false, false);
                }
                else
                {
                    await channel.BasicAckAsync(ea.DeliveryTag, false);
                    Console.WriteLine("TRANSACCION PROCESADA");
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error processing message: {ex.Message}");
                await channel.BasicNackAsync(ea.DeliveryTag, false, true);
            }
            finally            
            {
                _TransactionRateLimiter.Release();
            }
        }
        private async Task EnqueueToRetryQueue(string queue, IChannel channel, TransactionMessageDto transaction)
        {
            transaction.RetryCount += 1;
            var message = SerializeMessage(transaction);
            await channel.BasicPublishAsync(string.Empty, queue, message);
        }
        private async Task ExecuteTransactionRetry(BasicDeliverEventArgs ea, IChannel channel, string retryQueue)
        {
            try
            {
                
                var message = DeserializeMessage<TransactionMessageDto>(ea.Body);
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

                bool wasSent = await SendToDian(message.Payload);
                if (!wasSent)
                {
                    var body = SerializeMessage(message);
                    await channel.BasicNackAsync(ea.DeliveryTag, false, false);
                    await channel.BasicPublishAsync(string.Empty, retryQueue, body);
                }
                else
                {
                    await channel.BasicAckAsync(ea.DeliveryTag, false);
                    Console.WriteLine("REINTENTO PROCESADO");
                }
            }
            catch (Exception ex)
            {
                await channel.BasicNackAsync(ea.DeliveryTag, false, true);
                Console.WriteLine("ERROR REINTENTANDO: "+ex.Message);
            }
            finally
            {
                _retryRateLimiter.Release();
            }
           
        }
        private T DeserializeMessage<T>(ReadOnlyMemory<byte> messageBytes)
        {
            string jsonContent = Encoding.UTF8.GetString(messageBytes.ToArray());
            var deserializedMessage = JsonSerializer.Deserialize<T>(jsonContent);
            return deserializedMessage;
        }
        private byte[] SerializeMessage<T>(T message)
        {
            var messageString = JsonSerializer.Serialize(message);
            var messageBytes = Encoding.UTF8.GetBytes(messageString);
            return messageBytes;
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
    }
}
