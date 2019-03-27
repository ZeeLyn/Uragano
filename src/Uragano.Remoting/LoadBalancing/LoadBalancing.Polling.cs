using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Uragano.Abstractions;
using Uragano.Abstractions.Exceptions;
using Uragano.Abstractions.ServiceDiscovery;

namespace Uragano.Remoting.LoadBalancing
{
    public class LoadBalancingPolling : ILoadBalancing
    {
        private IServiceStatusManage ServiceStatusManageFactory { get; }

        private static int _index = -1;
        private static readonly object LockObject = new object();
        public LoadBalancingPolling(IServiceStatusManage serviceStatusManageFactory)
        {
            ServiceStatusManageFactory = serviceStatusManageFactory;
        }

        public async Task<ServiceNodeInfo> GetNextNode(string serviceName, string serviceRoute, object[] serviceArgs, Dictionary<string, string> serviceMeta)
        {
            var nodes = await ServiceStatusManageFactory.GetServiceNodes(serviceName);
            lock (LockObject)
            {
                if (!nodes.Any())
                    throw new NotFoundNodeException(serviceName);
                _index++;
                if (_index > nodes.Count - 1)
                    _index = 0;
                return nodes[_index];
            }
        }
    }
}
