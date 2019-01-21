using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Uragano.Abstractions.ServiceDiscovery
{
	public interface IServiceDiscovery
	{
		Task<bool> RegisterAsync(IServiceDiscoveryClientConfiguration serviceDiscoveryClientConfiguration, IServiceRegisterConfiguration serviceRegisterConfiguration, int? weight = default, CancellationToken cancellationToken = default);

		Task<bool> DeregisterAsync(IServiceDiscoveryClientConfiguration serviceDiscoveryClientConfiguration, string serviceId, CancellationToken cancellationToken = default);

		Task<List<ServiceDiscoveryInfo>> QueryServiceAsync(IServiceDiscoveryClientConfiguration serviceDiscoveryClientConfiguration, string serviceName, ServiceStatus serviceStatus = ServiceStatus.Alive, CancellationToken cancellationToken = default);
	}
}
