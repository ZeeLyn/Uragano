using Uragano.Abstractions.ServiceDiscovery;
using System;

namespace Uragano.Abstractions.LoadBalancing
{
	public abstract class ILoadBalancing
	{
		public static Type Polling = typeof(LoadBalancingPolling);

		public static Type WeightedPolling = typeof(LoadBalancingWeightedPolling);


		public abstract ServiceNodeInfo GetNextNode(string serviceName);
	}
}
