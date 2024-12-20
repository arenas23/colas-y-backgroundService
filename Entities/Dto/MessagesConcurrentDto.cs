using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Dto
{
    public class MessagesConcurrentDto
    {
        public ushort TransactionConcurrentMessages { get; set; }
        public ushort RetryConcurrentMessages { get; set; }
    }
}
