using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces.Publicer
{
    public interface IPublisher
    {
        Task Publish<T>(T message);
    }
}
