using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Uragano.Abstractions;
using Uragano.Abstractions.Exceptions;
using Uragano.Abstractions.ServiceDiscovery;

namespace Uragano.Remoting.LoadBalancing
{
    public class LoadBalancingRandom : ILoadBalancing
    {
        private IServiceStatusManage ServiceStatusManageFactory { get; }

        private static readonly object LockObject = new object();
        public LoadBalancingRandom(IServiceStatusManage serviceStatusManageFactory)
        {
            ServiceStatusManageFactory = serviceStatusManageFactory;
        }
        public async Task<ServiceNodeInfo> GetNextNode(string serviceName, string serviceRoute, object[] serviceArgs, Dictionary<string, string> serviceMeta)
        {
            var nodes = await ServiceStatusManageFactory.GetServiceNodes(serviceName);
            if (!nodes.Any())
                throw new NotFoundNodeException(serviceName);
            return nodes[new Random().Next(0, nodes.Count)];
        }
    }
}
