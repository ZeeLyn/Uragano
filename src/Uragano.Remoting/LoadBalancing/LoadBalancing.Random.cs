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
    public class LoadBalancingRandom : ILoadBalancing
    {
        private IServiceDiscovery ServiceDiscovery { get; }

        private ILogger Logger { get; }

        public LoadBalancingRandom(IServiceDiscovery serviceDiscovery,ILogger<LoadBalancingRandom> logger)
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
            var node= nodes[new Random().Next(0, nodes.Count)];
            if (Logger.IsEnabled(LogLevel.Trace))
                Logger.LogTrace($"Load to node {node.ServiceId}.");
            return node;
        }
    }
}
