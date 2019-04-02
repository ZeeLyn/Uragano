using Microsoft.Extensions.Configuration;
using Uragano.Abstractions;

namespace Uragano.Consul
{
    public static class UraganoBuilderExtensions
    {
        public static void AddConsul(this IUraganoBuilder builder, ConsulClientConfigure consulClientConfiguration)
        {
            builder.AddHostedService<ServiceStatusManageStartup>();
            builder.AddServiceDiscovery<ConsulServiceDiscovery>(consulClientConfiguration);
        }

        public static void AddConsul(this IUraganoBuilder builder, IConfigurationSection clientConfigurationSection)
        {
            builder.AddConsul(CommonMethods.ReadConsulClientConfigure(clientConfigurationSection));
        }

        public static void AddConsul(this IUraganoBuilder builder, ConsulClientConfigure consulClientConfiguration, ConsulRegisterServiceConfiguration consulAgentServiceConfiguration)
        {
            builder.AddHostedService<ServiceStatusManageStartup>();
            builder.AddServiceDiscovery<ConsulServiceDiscovery>(consulClientConfiguration, consulAgentServiceConfiguration);
        }

        public static void AddConsul(this IUraganoBuilder builder, IConfigurationSection clientConfigurationSection,
            IConfigurationSection serviceConfigurationSection)
        {
            var service = CommonMethods.ReadRegisterServiceConfiguration(serviceConfigurationSection);
            var client = CommonMethods.ReadConsulClientConfigure(clientConfigurationSection);
            builder.AddConsul(client, service);
        }

        public static void AddConsul(this IUraganoSampleBuilder builder)
        {
            var client = builder.Configuration.GetSection("Uragano:ServiceDiscovery:Consul:Client");
            var service = builder.Configuration.GetSection("Uragano:ServiceDiscovery:Consul:Service");
            if (service.Exists())
                builder.AddConsul(client, service);
            else
                builder.AddConsul(client);
        }
    }
}
