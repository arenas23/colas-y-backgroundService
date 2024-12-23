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
using System.Diagnostics;
using Domain.Interfaces.Publicer;


namespace Infrastructure.RabbitMqUtil
{
    public class Publisher : IPublisher
    {

        private readonly RabbitMqSettings _settings;
        private readonly ChannelManager _channelManager;

        public Publisher(IOptions<RabbitMqSettings> settings, ChannelManager channelManager) 
        {
            _settings = settings.Value;
            _channelManager = channelManager;
        }
        public async Task Publish<T>(T message)
        {
            var body = MessageHelper.SerializeMessage(message);
            await _channelManager.TransactionChannel.BasicPublishAsync(string.Empty, _settings.TransactionChannel.Queue, true, _channelManager.MessageProperties, body);
        }
        
    }
}
