using System;
using Microsoft.Extensions.Configuration;
using Uragano.Abstractions;

namespace Uragano.Consul
{
    public static class UraganoConfigurationExtensions
    {
        public static void AddConsul(this IUraganoConfiguration uraganoConfiguration,
            ConsulClientConfigure consulClientConfiguration)
        {
            uraganoConfiguration.AddServiceDiscovery<ConsulServiceDiscovery>(consulClientConfiguration);
        }

        public static void AddConsul(this IUraganoConfiguration uraganoConfiguration, IConfigurationSection clientConfigurationSection)
        {
            uraganoConfiguration.AddServiceDiscovery<ConsulServiceDiscovery>(CommonMethods.ReadConsulClientConfigure(clientConfigurationSection));
        }

        public static void AddConsul(this IUraganoConfiguration uraganoConfiguration,
            ConsulClientConfigure consulClientConfiguration,
            ConsulRegisterServiceConfiguration consulAgentServiceConfiguration)
        {
            if (string.IsNullOrWhiteSpace(consulAgentServiceConfiguration.Id))
            {
                consulAgentServiceConfiguration.Id = Guid.NewGuid().ToString("N");
            }
            uraganoConfiguration.AddServiceDiscovery<ConsulServiceDiscovery>(consulClientConfiguration, consulAgentServiceConfiguration);
        }

        public static void AddConsul(this IUraganoConfiguration uraganoConfiguration, IConfigurationSection clientConfigurationSection,
            IConfigurationSection serviceConfigurationSection)
        {
            var service = CommonMethods.ReadRegisterServiceConfiguration(serviceConfigurationSection);

            if (string.IsNullOrWhiteSpace(service.Id))
            {
                service.Id = Guid.NewGuid().ToString("N");
            }

            uraganoConfiguration.AddServiceDiscovery<ConsulServiceDiscovery>(CommonMethods.ReadConsulClientConfigure(clientConfigurationSection), service);
        }
    }
}
