using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Uragano.Abstractions;
using Uragano.Abstractions.Exceptions;
using Uragano.Abstractions.ServiceDiscovery;

namespace Uragano.Remoting.LoadBalancing
{
    public class LoadBalancingPolling : ILoadBalancing
    {
        private IServiceDiscovery ServiceDiscovery { get; }

        private static int _index = -1;
        private static readonly object LockObject = new object();

        private ILogger Logger { get; }
        public LoadBalancingPolling(IServiceDiscovery serviceDiscovery,ILogger<LoadBalancingPolling> logger)
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
                _index++;
                if (_index > nodes.Count - 1)
                    _index = 0;
                if(Logger.IsEnabled( LogLevel.Trace))
                    Logger.LogTrace($"Load to node {nodes[_index].ServiceId}.");
                return nodes[_index];
            }
        }
    }
}
