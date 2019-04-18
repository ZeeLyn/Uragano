using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Uragano.Abstractions;
using Uragano.Abstractions.ServiceDiscovery;

namespace Uragano.Core.HostedService
{
    public class ServiceDiscoveryStartup : IHostedService
    {
        private IServiceDiscovery ServiceDiscovery { get; }

        private ServerSettings ServerSettings { get; }

        public ServiceDiscoveryStartup(IServiceDiscovery serviceDiscovery, UraganoSettings uraganoSettings)
        {
            ServiceDiscovery = serviceDiscovery;
            ServerSettings = uraganoSettings.ServerSettings;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (ServerSettings == null)
                return;
            await ServiceDiscovery.RegisterAsync(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (ServerSettings == null)
                return;
            await ServiceDiscovery.DeregisterAsync();
        }
    }
}
