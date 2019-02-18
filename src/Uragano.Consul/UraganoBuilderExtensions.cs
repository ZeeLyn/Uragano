using System;
using Microsoft.Extensions.Configuration;
using Uragano.Abstractions;

namespace Uragano.Consul
{
    public static class UraganoBuilderExtensions
    {
        public static void AddConsul(this IUraganoBuilder builder, ConsulClientConfigure consulClientConfiguration)
        {
            builder.AddServiceDiscovery<ConsulServiceDiscovery>(consulClientConfiguration);
        }

        public static void AddConsul(this IUraganoBuilder builder, IConfigurationSection clientConfigurationSection)
        {
            builder.AddServiceDiscovery<ConsulServiceDiscovery>(CommonMethods.ReadConsulClientConfigure(clientConfigurationSection));
        }

        public static void AddConsul(this IUraganoBuilder builder, ConsulClientConfigure consulClientConfiguration, ConsulRegisterServiceConfiguration consulAgentServiceConfiguration)
        {
            if (string.IsNullOrWhiteSpace(consulAgentServiceConfiguration.Id))
            {
                consulAgentServiceConfiguration.Id = Guid.NewGuid().ToString("N");
            }
            builder.AddServiceDiscovery<ConsulServiceDiscovery>(consulClientConfiguration, consulAgentServiceConfiguration);
        }

        public static void AddConsul(this IUraganoBuilder builder, IConfigurationSection clientConfigurationSection,
            IConfigurationSection serviceConfigurationSection)
        {
            var service = CommonMethods.ReadRegisterServiceConfiguration(serviceConfigurationSection);

            if (string.IsNullOrWhiteSpace(service.Id))
            {
                service.Id = Guid.NewGuid().ToString("N");
            }

            builder.AddServiceDiscovery<ConsulServiceDiscovery>(CommonMethods.ReadConsulClientConfigure(clientConfigurationSection), service);
        }
    }
}
