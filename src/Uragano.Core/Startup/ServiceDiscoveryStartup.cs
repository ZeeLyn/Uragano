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
            if (UraganoSettings.ServiceRegisterConfiguration == null || UraganoSettings.IsDevelopment) return;
            if (UraganoSettings.ServiceDiscoveryClientConfiguration == null)
                throw new ArgumentNullException(nameof(UraganoSettings.ServiceDiscoveryClientConfiguration));

            await ServiceDiscovery.RegisterAsync(UraganoSettings.ServiceDiscoveryClientConfiguration, UraganoSettings.ServiceRegisterConfiguration, UraganoSettings.ServerSettings.Weight, cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (!UraganoSettings.IsDevelopment)
                await ServiceDiscovery.DeregisterAsync(UraganoSettings.ServiceDiscoveryClientConfiguration, UraganoSettings.ServiceRegisterConfiguration.Id, cancellationToken);
        }
    }
}
