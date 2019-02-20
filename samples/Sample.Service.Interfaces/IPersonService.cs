using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Uragano.Abstractions;

namespace Sample.Service.Interfaces
{
    [ServiceDiscoveryName("PersionService")]
    [ServiceRoute("persion")]
    public interface IPersonService : IService
    {
        [ServiceRoute("getname")]
        Task<object> GetName(int id);
    }
}
