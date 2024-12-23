using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Domain.Entities.Helpers
{
    public static class MessageHelper
    {
        public static T DeserializeMessage<T>(byte[] messageBytes)
        {
            try
            {
                string jsonContent = Encoding.UTF8.GetString(messageBytes);
                var deserializedMessage = JsonSerializer.Deserialize<T>(jsonContent);
                return deserializedMessage!;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
           
        }
        public static byte[] SerializeMessage<T>(T message)
        {
            var messageString = JsonSerializer.Serialize(message);
            var messageBytes = Encoding.UTF8.GetBytes(messageString);
            return messageBytes;
        }

        public static async Task WaitToRetry(int retries, CancellationToken cancelattionToken)
        {
            if (retries <= 3)
            {
                await Task.Delay(5000, cancelattionToken);
            }
            else if (retries <= 6)
            {
                await Task.Delay(10000, cancelattionToken);
            }
            else
            {
                await Task.Delay(15000, cancelattionToken);
            }
        }
    }
}
