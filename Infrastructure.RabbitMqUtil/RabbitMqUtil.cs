using Domain.Entities.Request;
using Domain.Interfaces.RabbitMqUtil;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using System.Threading.RateLimiting;
using System.Threading.Tasks;


namespace Infrastructure.RabbitMqUtil
{
    public class RabbitMqUtil : IRabbitMqUtil
    {
        private readonly SemaphoreSlim _rateLimiter;
        private const int MaxConcurrentTasks = 3;
        public RabbitMqUtil() 
        {
            _rateLimiter = new SemaphoreSlim(MaxConcurrentTasks, MaxConcurrentTasks);
        }
        public async Task PublishMessageQueue(string routingKey, byte[] body)
        {
            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest"
            };

            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();
            await channel.BasicPublishAsync("", routingKey, body);


        }

        public async Task ListenToQueueAsync(IChannel channel, CancellationToken cancellationToken)
        {
            var consumer = new AsyncEventingBasicConsumer(channel);
            var tasks = new List<Task>();

            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    await _rateLimiter.WaitAsync();
                    tasks.Add(ProcessMessageAsync(ea, channel));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing message: {ex.Message}");
                    await channel.BasicNackAsync(ea.DeliveryTag, false, true);
                }
            };

            await channel.BasicConsumeAsync("hello", false, consumer);
            Console.WriteLine("Started listening to the queue.");
            await Task.WhenAll(tasks);
        }

        private async Task ProcessMessageAsync(BasicDeliverEventArgs ea, IChannel channel)
        {
            try
            {
                string jsonString = Encoding.UTF8.GetString(ea.Body.ToArray());
                var request = JsonSerializer.Deserialize<PeajeRequest>(jsonString);
                Console.WriteLine("Procesando mensaje...");
                await Task.Delay(5000);
                await channel.BasicAckAsync(ea.DeliveryTag, false);
                Console.WriteLine("procesada");
            }
            finally            
            {
                _rateLimiter.Release();
            }
        }
    }
}
