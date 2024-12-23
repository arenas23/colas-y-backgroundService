using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces.Consumers
{
    public interface IConsumer
    {
        Task ListenToQueueAsync(CancellationToken cancellationToken);
        Task CloseChannel();
        Task OpenChannel(ushort concurrentMessages, CancellationToken cancelationToken);
    }
}
