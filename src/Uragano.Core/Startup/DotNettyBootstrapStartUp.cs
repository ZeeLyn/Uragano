using System.Threading;
using Microsoft.AspNetCore.Hosting;
using Uragano.Abstractions;
using Uragano.Remoting;

namespace Uragano.Core.Startup
{
    public class DotNettyBootstrapStartUp : IStartupTask
    {
        private UraganoSettings UraganoSettings { get; }
        private IBootstrap Bootstrap { get; }

        private IApplicationLifetime ApplicationLifetime { get; }

        private CancellationTokenSource CancellationTokenSource { get; }

        public DotNettyBootstrapStartUp(UraganoSettings uraganoSettings, IBootstrap bootstrap, IApplicationLifetime applicationLifetime, CancellationTokenSource cancellationTokenSource)
        {
            UraganoSettings = uraganoSettings;
            Bootstrap = bootstrap;
            ApplicationLifetime = applicationLifetime;
            CancellationTokenSource = cancellationTokenSource;
        }

        public void Execute()
        {
            if (UraganoSettings.ServerSettings == null) return;
            ApplicationLifetime.ApplicationStopping.Register(async () =>
            {
                CancellationTokenSource.Cancel();
                //if (!UraganoSettings.IsDevelopment)
                //    await discovery.DeregisterAsync(uraganoSettings.ServiceDiscoveryClientConfiguration, uraganoSettings.ServiceRegisterConfiguration.Id);
                await Bootstrap.StopAsync();
            });
            Bootstrap.StartAsync().Wait();
        }
    }
}
