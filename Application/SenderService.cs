
using Domain.Entities.Dto;
using Domain.Entities.Request;
using Domain.Interfaces;
using Domain.Interfaces.RabbitMqUtil;
using RabbitMQ.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace Services
{
    public class SenderService: ISenderService
    {
        private readonly IRabbitMqUtil _rabbitMqUtil;
        private IConnection? _connection;
        private IChannel? _transactionChannel;
        private Random _random;
        public SenderService(IRabbitMqUtil rabbitMqUtil)
        {
            _rabbitMqUtil = rabbitMqUtil;
            _random = new Random();

        }


        public async Task SendMessageToQueue(PeajeRequest request)
        {
            var message = new TransactionMessageDto
            {
                Payload = request,
                RetryCount = _random.Next(0,7),
                FirtAttemptTime = DateTime.UtcNow,
            };

            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest"
            };

            _connection = await factory.CreateConnectionAsync();

            _transactionChannel = await _connection.CreateChannelAsync();
            await _transactionChannel.QueueDeclareAsync("transaction-queue", false, false, false, null, false);
            for (var i = 0; i < 100; i++) 
            {
                await _rabbitMqUtil.Publish(_transactionChannel, "transaction-queue",message);
            }
            

            Console.WriteLine("enviado");
        }
    }
}
