using System;
using System.Linq;
using System.Threading.Tasks;
using Uragano.Abstractions;
using Uragano.Abstractions.ServiceDiscovery;

namespace Uragano.Remoting.LoadBalancing
{
	public class LoadBalancingPolling : ILoadBalancing
	{
		private IServiceStatusManageFactory ServiceStatusManageFactory { get; }

		private static int _index = -1;
		private static readonly object LockObject = new object();
		public LoadBalancingPolling(IServiceStatusManageFactory serviceStatusManageFactory)
		{
			ServiceStatusManageFactory = serviceStatusManageFactory;
		}

		public async Task<ServiceNodeInfo> GetNextNode(string serviceName)
		{
			var nodes = await ServiceStatusManageFactory.GetServiceNodes(serviceName);
			lock (LockObject)
			{
				if (!nodes.Any())
					throw new Exception($"Service {serviceName} did not find available nodes.");
				_index++;
				if (_index > nodes.Count - 1)
					_index = 0;
				return nodes[_index];
			}
		}
	}
}
