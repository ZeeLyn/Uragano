using System;
using System.Threading;
using Microsoft.AspNetCore.Hosting;
using Uragano.Abstractions;
using Uragano.Abstractions.ServiceDiscovery;
using Uragano.Remoting;

namespace Uragano.Core.Startup
{
    public class ServiceDiscoveryStartup : IStartupTask
    {
        private IApplicationLifetime ApplicationLifetime { get; }

        private IServiceDiscovery ServiceDiscovery { get; }

        private UraganoSettings UraganoSettings { get; }

        private IBootstrap Bootstrap { get; }

        private CancellationTokenSource CancellationTokenSource { get; }

        public ServiceDiscoveryStartup(IApplicationLifetime applicationLifetime, IServiceDiscovery serviceDiscovery, UraganoSettings uraganoSettings, IBootstrap bootstrap, CancellationTokenSource cancellationTokenSource)
        {
            ApplicationLifetime = applicationLifetime;
            ServiceDiscovery = serviceDiscovery;
            UraganoSettings = uraganoSettings;
            Bootstrap = bootstrap;
            CancellationTokenSource = cancellationTokenSource;
        }

        public void Execute()
        {
            if (UraganoSettings.ServiceRegisterConfiguration != null && !UraganoSettings.IsDevelopment)
            {
                if (UraganoSettings.ServiceDiscoveryClientConfiguration == null)
                    throw new ArgumentNullException(nameof(UraganoSettings.ServiceDiscoveryClientConfiguration));

                ServiceDiscovery.RegisterAsync(UraganoSettings.ServiceDiscoveryClientConfiguration, UraganoSettings.ServiceRegisterConfiguration, UraganoSettings.ServerSettings.Weight).Wait();
                ApplicationLifetime.ApplicationStopping.Register(async () =>
                {
                    CancellationTokenSource.Cancel();
                    if (!UraganoSettings.IsDevelopment)
                        await ServiceDiscovery.DeregisterAsync(UraganoSettings.ServiceDiscoveryClientConfiguration, UraganoSettings.ServiceRegisterConfiguration.Id);
                    await Bootstrap.StopAsync();
                });
            }


        }
    }
}
