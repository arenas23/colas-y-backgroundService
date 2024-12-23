using Infrastructure.RabbitMqUtil.Config;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.RabbitMqUtil
{
    public class ChannelManager
    {
        private readonly RabbitMqSettings _settings;
        private IConnection _connection;
        public IChannel TransactionChannel { get; private set; }
        public IChannel RetryChannel { get; private set; }
        public BasicProperties MessageProperties { get; private set; }
        public ChannelManager(IOptions<RabbitMqSettings> settings)
        {
            _settings = settings.Value;
            InitializeChannelsAsync(CancellationToken.None).Wait();
            MessageProperties = new() {Persistent = true };
        }

        private async Task InitializeChannelsAsync(CancellationToken cancellationToken)
        {
            var factory = new ConnectionFactory
            {
                HostName = _settings.HostName,
                UserName = _settings.UserName,
                Password = _settings.Password
            };

            _connection = await factory.CreateConnectionAsync(cancellationToken);

            await CreateTransactionChannelAsync(_settings.TransactionChannel.ConcurrentMessages, cancellationToken);
            await CreateRetryTransactionChannelAsync(_settings.RetryChannel.ConcurrentMessages, cancellationToken);
        }

        public async Task CloseTransactionChannel()
        {
            await TransactionChannel.CloseAsync();
        }

        public async Task CloseRetryTransactionChannel()
        {
            await RetryChannel.CloseAsync();
        }

        public async Task InitializeTransactionChannel(ushort concurrentMessages, CancellationToken cancellationToken)
        {
            await CreateTransactionChannelAsync(concurrentMessages, cancellationToken);
        }

        public async Task InitializeRetryTransactionChannel(ushort concurrentMessages, CancellationToken cancellationToken)
        {
           await CreateRetryTransactionChannelAsync(concurrentMessages, cancellationToken);
        }

        private async Task CreateTransactionChannelAsync(ushort concurrentMessages, CancellationToken cancellationToken)
        {
           
            TransactionChannel = await _connection.CreateChannelAsync(null, cancellationToken);
            await TransactionChannel.QueueDeclareAsync(_settings.TransactionChannel.Queue, true, false, false, null, false, cancellationToken);
            await TransactionChannel.BasicQosAsync(0, concurrentMessages, false, cancellationToken);
          
        }
        private async Task CreateRetryTransactionChannelAsync(ushort concurrentMessages, CancellationToken cancellationToken)
        {
            RetryChannel = await _connection.CreateChannelAsync(null, cancellationToken);
            await RetryChannel.QueueDeclareAsync(_settings.RetryChannel.Queue, true, false, false, null, false, cancellationToken);
            await RetryChannel.BasicQosAsync(0, concurrentMessages, false, cancellationToken);  
        }
    }
}
