using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Uragano.Abstractions;
using Uragano.Abstractions.ConsistentHash;
using Uragano.Abstractions.Exceptions;
using Uragano.Abstractions.ServiceDiscovery;

namespace Uragano.Remoting.LoadBalancing
{
    public class LoadBalancingConsistentHash : ILoadBalancing
    {
        private static readonly object LockObject = new object();

        private IServiceDiscovery ServiceDiscovery { get; }

        private static ConcurrentDictionary<string, IConsistentHash<ServiceNodeInfo>> ServicesInfo { get; } = new ConcurrentDictionary<string, IConsistentHash<ServiceNodeInfo>>();

        public LoadBalancingConsistentHash(IServiceDiscovery serviceDiscovery)
        {
            ServiceDiscovery.OnNodeJoin += OnNodeJoin;
            ServiceDiscovery.OnNodeLeave += OnNodeLeave;
            ServiceDiscovery = serviceDiscovery;
        }


        private void OnNodeLeave(string serviceName, IReadOnlyList<string> servicesId)
        {
            if (!servicesId.Any()) return;
            if (!ServicesInfo.TryGetValue(serviceName, out var service)) return;
            foreach (var node in servicesId)
            {
                service.RemoveNode(node);
            }
        }

        private void OnNodeJoin(string serviceName, IReadOnlyList<ServiceNodeInfo> nodes)
        {
            if (!nodes.Any())
                return;
            if (!ServicesInfo.TryGetValue(serviceName, out var service)) return;
            foreach (var node in nodes)
            {
                service.AddNode(node, node.ServiceId);
            }
        }

        public async Task<ServiceNodeInfo> GetNextNode(string serviceName, string serviceRoute, IReadOnlyList<object> serviceArgs,
            IReadOnlyDictionary<string, string> serviceMeta)
        {
            var nodes = await ServiceDiscovery.GetServiceNodes(serviceName);
            if (!nodes.Any())
                throw new NotFoundNodeException(serviceName);
            if (nodes.Count == 1)
                return nodes.First();
            lock (LockObject)
            {
                if (serviceMeta == null || !serviceMeta.TryGetValue("x-consistent-hash-key", out var key) || string.IsNullOrWhiteSpace(key))
                {
                    throw new ArgumentNullException(nameof(serviceMeta), "Service metadata [x-consistent-hash-key]  is empty,Please call SetMeta method to pass in.");
                }

                return ServicesInfo.GetOrAdd(serviceName, k =>
                {
                    var consistentHash = new ConsistentHash<ServiceNodeInfo>();
                    foreach (var node in nodes)
                    {
                        consistentHash.AddNode(node, node.ServiceId);
                    }

                    ServicesInfo.TryAdd(serviceName, consistentHash);
                    return consistentHash;
                }).GetNodeForKey(key);
            }
        }
    }
}
