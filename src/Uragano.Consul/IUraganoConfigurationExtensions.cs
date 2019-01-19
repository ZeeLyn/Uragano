using Uragano.Abstractions;

namespace Uragano.Consul
{
	public static class IUraganoConfigurationExtensions
	{
		public static void AddConsul(this IUraganoConfiguration uraganoConfiguration,
			ConsulClientConfigure consulClientConfiguration)
		{
			uraganoConfiguration.AddServiceDiscovery<ConsulServiceDiscovery>(consulClientConfiguration);
		}

		public static void AddConsul(this IUraganoConfiguration uraganoConfiguration,
			ConsulClientConfigure consulClientConfiguration,
			ConsulRegisterServiceConfiguration consulAgentServiceConfiguration)
		{
			uraganoConfiguration.AddServiceDiscovery<ConsulServiceDiscovery>(consulClientConfiguration, consulAgentServiceConfiguration);
		}
	}
}
