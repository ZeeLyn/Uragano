using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Uragano.Abstractions.ServiceDiscovery
{
    public interface IServiceDiscovery
    {

        IServiceDiscoveryClientConfiguration ReadClientConfiguration(IConfigurationSection clientConfigurationSection);

        IServiceRegisterConfiguration ReadServiceRegisterConfiguration(IConfigurationSection serviceConfigurationSection);


        Task<bool> RegisterAsync(IServiceDiscoveryClientConfiguration serviceDiscoveryClientConfiguration, IServiceRegisterConfiguration serviceRegisterConfiguration, int? weight = default, CancellationToken cancellationToken = default);

        Task<bool> DeregisterAsync(IServiceDiscoveryClientConfiguration serviceDiscoveryClientConfiguration, string serviceId, CancellationToken cancellationToken = default);

        Task<List<ServiceDiscoveryInfo>> QueryServiceAsync(IServiceDiscoveryClientConfiguration serviceDiscoveryClientConfiguration, string serviceName, ServiceStatus serviceStatus = ServiceStatus.Alive, CancellationToken cancellationToken = default);
    }
}
