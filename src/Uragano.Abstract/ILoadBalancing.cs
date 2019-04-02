using System.Collections.Generic;
using System.Threading.Tasks;
using Uragano.Abstractions.ServiceDiscovery;

namespace Uragano.Abstractions
{
    public interface ILoadBalancing
    {
        Task<ServiceNodeInfo> GetNextNode(string serviceName, string serviceRoute, IReadOnlyList<object> serviceArgs, IReadOnlyDictionary<string, string> serviceMeta);
    }
}
