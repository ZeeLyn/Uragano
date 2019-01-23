using System;
using System.Linq;
using Uragano.Abstractions.ServiceDiscovery;

namespace Uragano.Abstractions.LoadBalancing
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

		public override ServiceNodeInfo GetNextNode(string serviceName)
		{
			//lock (LockObject)
			//{
			var nodes = ServiceStatusManageFactory.GetServiceNodes(serviceName);
			if (!nodes.Any())
				throw new Exception($"Service {serviceName} did not find available nodes.");
			_index++;
			if (_index > nodes.Count - 1)
				_index = 0;
			return nodes[_index];
			//}
		}
	}
}
