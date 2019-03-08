using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Uragano.Abstractions;
using Uragano.Remoting;

namespace Uragano.Core.HostedService
{
    public class BootstrapStartup : IHostedService
    {
        private UraganoSettings UraganoSettings { get; }
        private IBootstrap Bootstrap { get; }

        public BootstrapStartup(UraganoSettings uraganoSettings, IBootstrap bootstrap)
        {
            UraganoSettings = uraganoSettings;
            Bootstrap = bootstrap;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (UraganoSettings.ServerSettings == null) return;
            await Bootstrap.StartAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Bootstrap.StopAsync();
        }
    }
}
