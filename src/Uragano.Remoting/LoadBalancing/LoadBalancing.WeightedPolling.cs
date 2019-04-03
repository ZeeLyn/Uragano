using Microsoft.Extensions.Logging;
using System;
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
        private static readonly object LockObject = new object();

        private IServiceDiscovery ServiceDiscovery { get; }

        private const string Key = "x-weighted-polling-current-value";

        private ILogger Logger { get; }

        public LoadBalancingWeightedPolling(IServiceDiscovery serviceDiscovery,ILogger<LoadBalancingWeightedPolling> logger)
        {
            ServiceDiscovery = serviceDiscovery;
            Logger = logger;
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

                var index = -1;
                var total = 0;
                for (var i = 0; i < nodes.Count; i++)
                {
                    nodes[i].Attach.AddOrUpdate(Key, nodes[i].Weight, (key, old) => Convert.ToInt32(old) + nodes[i].Weight);
                    total += nodes[i].Weight;
                    if (index == -1 || GetCurrentWeightValue(nodes[index]) < GetCurrentWeightValue(nodes[i]))
                    {
                        index = i;
                    }
                }
                nodes[index].Attach.AddOrUpdate(Key, -total, (k, old) => Convert.ToInt32(old) - total);
                var node= nodes[index];
                if (Logger.IsEnabled(LogLevel.Trace))
                    Logger.LogTrace($"Load to node {node.ServiceId}.");
                return node;
            }
        }

        private static int GetCurrentWeightValue(ServiceNodeInfo node)
        {
            return !node.Attach.TryGetValue(Key, out var val) ? 0 : Convert.ToInt32(val);
        }
    }
}
