using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.RabbitMqUtil.Config
{
    public class RabbitMqSettings
    {
        public string HostName {  get; set; } =string.Empty;
        public string UserName {  get; set; } =string.Empty;
        public string Password { get; set; } = string.Empty;
        public ChannelSettings TransactionChannel { get; set; } = new();
        public ChannelSettings RetryChannel { get; set; } = new();
    }
}
