using System.Threading.Tasks;
using Uragano.Abstractions.ServiceDiscovery;

namespace Uragano.Abstractions
{
	public interface ILoadBalancing
	{
		//public static Type Polling = typeof(LoadBalancingPolling);

		//public static Type WeightedPolling = typeof(LoadBalancingWeightedPolling);


		Task<ServiceNodeInfo> GetNextNode(string serviceName);
	}
}
