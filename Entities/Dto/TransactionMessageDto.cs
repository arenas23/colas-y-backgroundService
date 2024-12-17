using Domain.Entities.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Dto
{
    public class TransactionMessageDto
    {
        public PeajeRequest Payload { get; set; } = new();
        public int RetryCount { get; set; }
        public DateTime FirtAttemptTime { get; set; }
    }
}
