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
        Task CloseChannels();
        Task Prendalo(CancellationToken cancelationToken);
    }
}
