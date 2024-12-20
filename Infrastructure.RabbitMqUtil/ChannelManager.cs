using Infrastructure.RabbitMqUtil.Config;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.RabbitMqUtil
{
    public class ChannelManager
    {
        private readonly RabbitMqSettings _settings;
        private IConnection _connection;
        public IChannel TransactionChannel { get; private set; }
        public IChannel RetryChannel { get; private set; }
        public ChannelManager(IOptions<RabbitMqSettings> settings)
        {
            _settings = settings.Value;
            InitializeChannelsAsync(CancellationToken.None).Wait();
        }

        public async Task InitializeChannelsAsync(CancellationToken cancellationToken)
        {
            var factory = new ConnectionFactory
            {
                HostName = _settings.HostName,
                UserName = _settings.UserName,
                Password = _settings.Password
            };

            _connection = await factory.CreateConnectionAsync(cancellationToken);

            TransactionChannel = await _connection.CreateChannelAsync(null, cancellationToken);
            await TransactionChannel.QueueDeclareAsync(_settings.TransactionChannel.Queue, false, false, false, null, false, cancellationToken);
            await TransactionChannel.BasicQosAsync(0, _settings.TransactionChannel.ConcurrentMessages, false, cancellationToken);

            RetryChannel = await _connection.CreateChannelAsync(null, cancellationToken);
            await RetryChannel.QueueDeclareAsync(_settings.RetryChannel.Queue, false, false, false, null, false, cancellationToken);
            await RetryChannel.BasicQosAsync(0, _settings.RetryChannel.ConcurrentMessages, false, cancellationToken);
        }

        public async Task CloseChannels()
        {
            await TransactionChannel?.CloseAsync();
            await RetryChannel?.CloseAsync();
            await _connection?.CloseAsync();
        }
    }
}
