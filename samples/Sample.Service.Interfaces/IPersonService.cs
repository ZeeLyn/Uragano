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
        [CircuitBreaker(FallbackExecuteScript = "return new{name=\"fall\"};", Retry = 2, TimeoutMilliseconds = 1000, MaxParallelization = 1, ExceptionsAllowedBeforeBreaking = 10)]
        Task<object> GetName(int id);
    }
}
