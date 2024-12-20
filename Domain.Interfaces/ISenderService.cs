using Domain.Entities.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface ISenderService
    {
        Task SendMessageToQueue(PeajeRequest message);
        void ChangeConcurrency();
    }
}
