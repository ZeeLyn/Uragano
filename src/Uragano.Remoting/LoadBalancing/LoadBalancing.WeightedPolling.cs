using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Uragano.Abstractions;
using Uragano.Abstractions.Exceptions;
using Uragano.Abstractions.ServiceDiscovery;

namespace Uragano.Remoting.LoadBalancing
{
    public class LoadBalancingWeightedPolling : ILoadBalancing
    {
        private IServiceStatusManage ServiceStatusManageFactory { get; }

        private static readonly object LockObject = new object();
        public LoadBalancingWeightedPolling(IServiceStatusManage serviceStatusManageFactory)
        {
            ServiceStatusManageFactory = serviceStatusManageFactory;
        }
        public async Task<ServiceNodeInfo> GetNextNode(string serviceName, string serviceRoute, object[] serviceArgs, Dictionary<string, string> serviceMeta)
        {
            var nodes = await ServiceStatusManageFactory.GetServiceNodes(serviceName);
            if (!nodes.Any())
                throw new NotFoundNodeException(serviceName);
            lock (LockObject)
            {
                var index = -1;
                var total = 0;
                for (var i = 0; i < nodes.Count; i++)
                {
                    nodes[i].CurrentWeight += nodes[i].Weight;
                    total += nodes[i].Weight;
                    if (index == -1 || nodes[index].CurrentWeight < nodes[i].CurrentWeight)
                    {
                        index = i;
                    }
                }
                nodes[index].CurrentWeight -= total;
                return nodes[index];
            }
        }
    }
}
