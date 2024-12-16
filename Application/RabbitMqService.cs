using Domain.Interfaces.RabbitMqUtil;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class RabbitMqService : BackgroundService
    {
        private readonly IRabbitMqUtil _rabbitMqUtil;
        private IChannel? _channel;
        private IConnection? _connection;
        public RabbitMqService(IRabbitMqUtil rabbitMqUtil)
        {
            _rabbitMqUtil = rabbitMqUtil;
        }
        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest"
            };

            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();
            await _channel.QueueDeclareAsync(
                queue: "hello",
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            await _channel.BasicQosAsync(0, 3, false, cancellationToken);
            Console.WriteLine("RabbitMqService started.");

            await _rabbitMqUtil.ListenToQueueAsync(_channel,cancellationToken);
            await base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //while (!stoppingToken.IsCancellationRequested)
            //{
            //    await _rabbitMqUtil.ListenMessageQueue(_channel!, "hello", stoppingToken);
            //}

            await Task.CompletedTask;

        }
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
            await _connection!.CloseAsync();
            Console.WriteLine("me detuve");
        }

        
    }
}
