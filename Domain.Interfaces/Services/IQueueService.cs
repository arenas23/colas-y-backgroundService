using Domain.Entities.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces.Services
{
    public interface IQueueService
    {
        Task SendMessageToQueue(PeajeRequest message);
        Task StopConsumers();
        Task TurnOnConsumers();
    }
}
