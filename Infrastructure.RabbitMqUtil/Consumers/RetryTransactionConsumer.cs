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
using System.Diagnostics;
using System.Text;
using System.Text.Json;


namespace Infrastructure.RabbitMqUtil.Consumers
{
    public class RetryTransactionConsumer: IConsumer
    {
        private SemaphoreSlim _rateLimiter;
        private readonly RabbitMqSettings _settings;
        private readonly ChannelManager _channelManager;
        private readonly IServiceProvider _serviceProvider;

        public RetryTransactionConsumer(IOptions<RabbitMqSettings> settings, ChannelManager channelManager, IServiceProvider serviceProvider)
        {
            _rateLimiter = new(settings.Value.RetryChannel.ConcurrentMessages, settings.Value.RetryChannel.ConcurrentMessages); ;
            _settings = settings.Value;
            _channelManager = channelManager;
            _serviceProvider = serviceProvider;
        }

        public async Task ListenToQueueAsync(CancellationToken cancellationToken)
        {
            var consumer = new AsyncEventingBasicConsumer(_channelManager.RetryChannel);
            var tasks = new List<Task>();
            Console.WriteLine("nueva config retry " + _rateLimiter.CurrentCount);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                await _rateLimiter.WaitAsync(cancellationToken);
                tasks.Add(ExecuteTransactionRetry(ea, cancellationToken));
            };

            await _channelManager.RetryChannel.BasicConsumeAsync(_settings.RetryChannel.Queue, false, consumer, cancellationToken);
            Console.WriteLine("Started listening to the RetryQueue.");
        }

        private async Task ExecuteTransactionRetry(BasicDeliverEventArgs ea, CancellationToken cancellationToken)
        {
            var argument = ea.Body.ToArray();
            var message = MessageHelper.DeserializeMessage<TransactionMessageDto>(argument);
            message.RetryCount += 1;

            await MessageHelper.WaitToRetry(message.RetryCount, cancellationToken);

            try
            {
                var dianApi = _serviceProvider.GetRequiredService<IDianApi>();
                bool wasSent = await dianApi.SendToDian(message.Payload, cancellationToken);
                if (!wasSent)
                {
                    var body = MessageHelper.SerializeMessage(message);
                    await _channelManager.RetryChannel.BasicNackAsync(ea.DeliveryTag, false, false,cancellationToken);
                    await _channelManager.RetryChannel.BasicPublishAsync(string.Empty, _settings.RetryChannel.Queue, true, _channelManager.MessageProperties, body, cancellationToken);
                    Console.WriteLine("Reintentar");
                }
                else
                {
                    await _channelManager.RetryChannel.BasicAckAsync(ea.DeliveryTag, false, cancellationToken);
                    Console.WriteLine("REINTENTO PROCESADO");
                }
            }
            catch (Exception ex)
            {
                var body = MessageHelper.SerializeMessage(message);
                await _channelManager.RetryChannel.BasicPublishAsync(string.Empty, _settings.RetryChannel.Queue, true, _channelManager.MessageProperties, body, cancellationToken);
                await _channelManager.RetryChannel.BasicNackAsync(ea.DeliveryTag, false, false, cancellationToken);
                Console.WriteLine("error Api Reintento");

            }
            finally
            {
                _rateLimiter.Release();
            }


        }
        public async Task CloseChannel()
        {
            await _channelManager.CloseRetryTransactionChannel();
        }

        public async Task OpenChannel(ushort concurrentMessages, CancellationToken cancelationToken)
        {
            _rateLimiter = new(concurrentMessages,concurrentMessages);
            await _channelManager.InitializeRetryTransactionChannel(concurrentMessages, cancelationToken);
        }
    }
}
