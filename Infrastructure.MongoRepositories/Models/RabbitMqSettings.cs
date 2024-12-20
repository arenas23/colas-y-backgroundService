using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.MongoDB.Models
{
    public class RabbitMqSettings
    {
        public ObjectId Id { get; set; }
        public ushort TransactionConcurrentMessages {  get; set; }
        public ushort RetryConcurrentMessages { get; set; }
    }
}
