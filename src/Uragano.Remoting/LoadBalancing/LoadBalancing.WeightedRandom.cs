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
    public class LoadBalancingWeightedRandom : ILoadBalancing
    {
        private static readonly object LockObject = new object();

        private IServiceDiscovery ServiceDiscovery { get; }

        private ILogger Logger { get; }

        public LoadBalancingWeightedRandom(IServiceDiscovery serviceDiscovery,ILogger<LoadBalancingWeightedRandom> logger)
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
                var total = nodes.Sum(p => p.Weight);
                var offset = new Random().Next(0, total+1);
                var currentSum = 0;
                foreach (var node in nodes.OrderByDescending(p => p.Weight))
                {
                    currentSum += node.Weight;
                    if (offset <= currentSum)
                    {
                        if (Logger.IsEnabled(LogLevel.Trace))
                            Logger.LogTrace($"Load to node {node.ServiceId}.");
                        return node;
                    }
                }
                var result= nodes[new Random().Next(0, nodes.Count)];
                if (Logger.IsEnabled(LogLevel.Trace))
                    Logger.LogTrace($"Load to node {result.ServiceId}.");
                return result;
            }
        }
    }
}
