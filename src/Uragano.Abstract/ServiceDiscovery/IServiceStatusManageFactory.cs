using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Uragano.Abstractions.ServiceDiscovery
{
	public interface IServiceStatusManageFactory
	{
		Task<List<ServiceNodeInfo>> GetServiceNodes(string serviceName, bool alive = true);

		Task Refresh(CancellationToken cancellationToken);
	}
}
