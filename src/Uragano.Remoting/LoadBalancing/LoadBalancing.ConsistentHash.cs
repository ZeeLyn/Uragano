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
        private IServiceStatusManage ServiceStatusManageFactory { get; }

        private static readonly object LockObject = new object();

        private static ConcurrentDictionary<string, IConsistentHash<ServiceNodeInfo>> ServicesInfo { get; } = new ConcurrentDictionary<string, IConsistentHash<ServiceNodeInfo>>();

        public LoadBalancingConsistentHash(IServiceStatusManage serviceStatusManageFactory)
        {
            ServiceStatusManageFactory = serviceStatusManageFactory;
            ServiceStatusManageFactory.OnNodeJoin += OnNodeJoin;
            ServiceStatusManageFactory.OnNodeLeave += OnNodeLeave;
        }


        private void OnNodeLeave(string serviceName, params ServiceNodeInfo[] nodeInfo)
        {
            if (!nodeInfo.Any()) return;
            if (!ServicesInfo.TryGetValue(serviceName, out var service)) return;
            foreach (var node in nodeInfo)
            {
                service.RemoveNode(node.ServiceId);
            }
        }

        private void OnNodeJoin(string serviceName, params ServiceNodeInfo[] nodeInfo)
        {
            if (!nodeInfo.Any())
                return;
            if (!ServicesInfo.TryGetValue(serviceName, out var service)) return;
            foreach (var node in nodeInfo)
            {
                service.AddNode(node, node.ServiceId);
            }
        }

        public async Task<ServiceNodeInfo> GetNextNode(string serviceName, string serviceRoute, object[] serviceArgs,
            Dictionary<string, string> serviceMeta)
        {
            var nodes = await ServiceStatusManageFactory.GetServiceNodes(serviceName);
            if (!nodes.Any())
                throw new NotFoundNodeException(serviceName);

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
