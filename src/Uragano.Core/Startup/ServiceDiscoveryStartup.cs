using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Uragano.Abstractions;
using Uragano.Abstractions.ServiceDiscovery;


namespace Uragano.Core.Startup
{
    public class ServiceDiscoveryStartup : IHostedService
    {
        private IServiceDiscovery ServiceDiscovery { get; }

        private UraganoSettings UraganoSettings { get; }

        public ServiceDiscoveryStartup(IServiceDiscovery serviceDiscovery, UraganoSettings uraganoSettings)
        {
            ServiceDiscovery = serviceDiscovery;
            UraganoSettings = uraganoSettings;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (UraganoSettings.ServerSettings == null)
                return;
            if (UraganoSettings.ServiceRegisterConfiguration == null) return;
            if (UraganoSettings.ServiceDiscoveryClientConfiguration == null)
                throw new ArgumentNullException(nameof(UraganoSettings.ServiceDiscoveryClientConfiguration));

            if (string.IsNullOrWhiteSpace(UraganoSettings.ServiceRegisterConfiguration.Id))
            {
                UraganoSettings.ServiceRegisterConfiguration.Id =
                    $"{UraganoSettings.ServerSettings.IP}:{UraganoSettings.ServerSettings.Port}";
            }

            if (string.IsNullOrWhiteSpace(UraganoSettings.ServiceRegisterConfiguration.Name))
            {
                UraganoSettings.ServiceRegisterConfiguration.Name = $"{UraganoSettings.ServerSettings.IP}:{UraganoSettings.ServerSettings.Port}";
            }

            await ServiceDiscovery.RegisterAsync(UraganoSettings.ServiceDiscoveryClientConfiguration, UraganoSettings.ServiceRegisterConfiguration, UraganoSettings.ServerSettings.Weight, cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (UraganoSettings.ServerSettings == null)
                return;
            await ServiceDiscovery.DeregisterAsync(UraganoSettings.ServiceDiscoveryClientConfiguration, UraganoSettings.ServiceRegisterConfiguration.Id);
        }
    }
}
