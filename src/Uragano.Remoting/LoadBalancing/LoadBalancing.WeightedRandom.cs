using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Uragano.Abstractions;
using Uragano.Abstractions.Exceptions;
using Uragano.Abstractions.ServiceDiscovery;

namespace Uragano.Remoting.LoadBalancing
{
    public class LoadBalancingWeightedRandom : ILoadBalancing
    {
        private static readonly object LockObject = new object();

        private IServiceDiscovery ServiceDiscovery { get; }
        public LoadBalancingWeightedRandom(IServiceDiscovery serviceDiscovery)
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
            lock (LockObject)
            {
                var total = nodes.Sum(p => p.Weight);
                var offset = new Random().Next(0, total);
                var currentSum = 0;
                foreach (var node in nodes.OrderByDescending(p => p.Weight))
                {
                    currentSum += node.Weight;
                    if (offset <= currentSum)
                        return node;
                }
                return nodes[new Random().Next(0, nodes.Count)];
            }
        }
    }
}
