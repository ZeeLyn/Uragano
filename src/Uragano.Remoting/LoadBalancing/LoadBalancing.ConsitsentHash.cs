using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Uragano.Abstractions;
using Uragano.Abstractions.ConsistentHash;
using Uragano.Abstractions.ServiceDiscovery;

namespace Uragano.Remoting.LoadBalancing
{
    public class LoadBalancingConsistentHash : ILoadBalancing
    {
        private IServiceStatusManage ServiceStatusManageFactory { get; }

        private ConsistentHash<ServiceNodeInfo> ConsistentHash { get; }
        private static readonly object LockObject = new object();

        private static ConcurrentDictionary<string, ServiceInfo> ServicesInfo { get; } = new ConcurrentDictionary<string, ServiceInfo>();
        public LoadBalancingConsistentHash(IServiceStatusManage serviceStatusManageFactory)
        {
            ServiceStatusManageFactory = serviceStatusManageFactory;
            ConsistentHash = new ConsistentHash<ServiceNodeInfo>();
        }

        public async Task<ServiceNodeInfo> GetNextNode(string serviceName, string serviceRoute, object[] serviceArgs,
            Dictionary<string, string> serviceMeta)
        {
            var nodes = await ServiceStatusManageFactory.GetServiceNodes(serviceName);
            lock (LockObject)
            {
                if (serviceMeta == null || !serviceMeta.TryGetValue("x-consistent-hash-key", out var key))
                {
                    throw new ArgumentNullException(nameof(serviceMeta), "Service metadata [x-consistent-hash-key] not found,Please call SetMeta method to pass in.");
                }
                if (string.IsNullOrWhiteSpace(key))
                    throw new ArgumentNullException(nameof(serviceMeta), "Service metadata [x-consistent-hash-key] is empty,Please call SetMeta method to pass in.");

                if (!ServicesInfo.TryGetValue(serviceName, out var info))
                {
                    var consistentHash = new ConsistentHash<ServiceNodeInfo>();
                    ServicesInfo.TryAdd(serviceName, new ServiceInfo
                    {
                        ConsistentHash = consistentHash,
                        Nodes = nodes
                    });
                    foreach (var node in nodes)
                    {
                        consistentHash.AddNode(node, node.ServiceId);
                    }

                    return consistentHash.GetNodeForKey(key);
                }

                var removedNodes = info.Nodes.FindAll(p => nodes.All(i => i != p));
                foreach (var node in removedNodes)
                {
                    info.ConsistentHash.RemoveNode(node.ServiceId);
                    info.Nodes.Remove(node);
                }

                var addedNodes = nodes.FindAll(p => info.Nodes.All(i => i != p));
                foreach (var node in addedNodes)
                {
                    info.ConsistentHash.AddNode(node, node.ServiceId);
                    info.Nodes.Add(node);
                }

                return info.ConsistentHash.GetNodeForKey(key);
            }
        }

        private class ServiceInfo
        {
            public ConsistentHash<ServiceNodeInfo> ConsistentHash { get; set; }

            public List<ServiceNodeInfo> Nodes { get; set; }
        }
    }
}
