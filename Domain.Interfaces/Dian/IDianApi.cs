using Domain.Entities.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces.Dian
{
    public interface IDianApi
    {
        Task<bool> SendToDian(PeajeRequest transaction, CancellationToken cancellationToken);
    }
}
