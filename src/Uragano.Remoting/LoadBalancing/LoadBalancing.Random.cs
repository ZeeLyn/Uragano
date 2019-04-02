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
        private IServiceDiscovery ServiceDiscovery { get; }

        public LoadBalancingRandom(IServiceDiscovery serviceDiscovery)
        {
            ServiceDiscovery = serviceDiscovery;
        }
        public async Task<ServiceNodeInfo> GetNextNode(string serviceName, string serviceRoute, IReadOnlyList<object> serviceArgs, IReadOnlyDictionary<string, string> serviceMeta)
        {
            var nodes = await ServiceDiscovery.GetServiceNodes(serviceName);
            if (!nodes.Any())
                throw new NotFoundNodeException(serviceName);
            if (nodes.Count == 1)
                return nodes.First();
            return nodes[new Random().Next(0, nodes.Count)];
        }
    }
}
