using Consul;
using Uragano.Abstractions.ServiceDiscovery;

namespace Uragano.Consul
{
	public class ConsulClientConfigure : ConsulClientConfiguration, IServiceDiscoveryClientConfiguration
	{
		public ConsulClientConfigure()
		{
		}

		public ConsulClientConfigure(string address, string token = "")
		{
			Address = new System.Uri(address);
			Token = token;
		}
	}
}
