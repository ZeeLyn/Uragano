using System;
using Microsoft.AspNetCore.Hosting;
using Uragano.Abstractions;
using Uragano.Abstractions.ServiceDiscovery;

namespace Uragano.Core.Startup
{
    public class ServiceDiscoveryStartup : IStartupTask
    {
        private IApplicationLifetime ApplicationLifetime { get; }

        private IServiceDiscovery ServiceDiscovery { get; }

        private UraganoSettings UraganoSettings { get; }

        public ServiceDiscoveryStartup(IApplicationLifetime applicationLifetime, IServiceDiscovery serviceDiscovery, UraganoSettings uraganoSettings)
        {
            ApplicationLifetime = applicationLifetime;
            ServiceDiscovery = serviceDiscovery;
            UraganoSettings = uraganoSettings;
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
                    if (!UraganoSettings.IsDevelopment)
                        await ServiceDiscovery.DeregisterAsync(UraganoSettings.ServiceDiscoveryClientConfiguration, UraganoSettings.ServiceRegisterConfiguration.Id);
                });
            }


        }
    }
}
