using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Uragano.Abstractions.ServiceDiscovery;

namespace Uragano.Abstractions.LoadBalancing
{
	public class LoadBalancingWeightedPolling : ILoadBalancing
	{
		private IServiceStatusManageFactory ServiceStatusManageFactory { get; }

		private static readonly object LockObject = new object();
		public LoadBalancingWeightedPolling(IServiceStatusManageFactory serviceStatusManageFactory)
		{
			ServiceStatusManageFactory = serviceStatusManageFactory;
		}
		public override ServiceNodeInfo GetNextNode(string serviceName)
		{
			lock (LockObject)
			{
				var nodes = ServiceStatusManageFactory.GetServiceNodes(serviceName);
				if (!nodes.Any())
					throw new Exception($"Service {serviceName} did not find available nodes.");
				var index = -1;
				var total = 0;
				for (var i = 0; i < nodes.Count; i++)
				{
					nodes[i].CurrentWeight += nodes[i].Weight;
					total += nodes[i].Weight;
					if (index == -1 || nodes[index].CurrentWeight < nodes[i].CurrentWeight)
					{
						index = i;
					}
				}

				nodes[index].CurrentWeight -= total;
				return nodes[index];
			}
		}
	}
}
