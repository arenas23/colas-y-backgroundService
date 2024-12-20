using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.RabbitMqUtil.Config
{
    public class ChannelSettings
    {
        public string Queue { get; set; } = string.Empty;
        public ushort ConcurrentMessages {  get; set; }
    }
}
