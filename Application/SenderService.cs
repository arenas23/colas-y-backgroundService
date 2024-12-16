
using Domain.Entities.Request;
using Domain.Interfaces;
using Domain.Interfaces.RabbitMqUtil;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Services
{
    public class SenderService: ISenderService
    {
        private readonly IRabbitMqUtil _rabbitMqUtil;
        public SenderService(IRabbitMqUtil rabbitMqUtil)
        {
            _rabbitMqUtil = rabbitMqUtil;
        }


        public async Task SendMessageToQueue(PeajeRequest message)
        {
            var body = ObjectToByteArray(message);

            await _rabbitMqUtil.PublishMessageQueue("hello", body);

            Console.WriteLine("enviado");
        }

        private byte[] ObjectToByteArray(object obj)
        {
            string jsonString = JsonSerializer.Serialize(obj);
            return Encoding.UTF8.GetBytes(jsonString);
        }
    }
}
