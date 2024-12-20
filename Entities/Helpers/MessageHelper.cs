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
        public static T DeserializeMessage<T>(ReadOnlyMemory<byte> messageBytes)
        {
            string jsonContent = Encoding.UTF8.GetString(messageBytes.ToArray());
            var deserializedMessage = JsonSerializer.Deserialize<T>(jsonContent);
            return deserializedMessage;
        }
        public static byte[] SerializeMessage<T>(T message)
        {
            var messageString = JsonSerializer.Serialize(message);
            var messageBytes = Encoding.UTF8.GetBytes(messageString);
            return messageBytes;
        }
    }
}
