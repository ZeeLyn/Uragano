using System;
using System.Collections.Generic;
using Uragano.Abstractions.ServiceDiscovery;

namespace Uragano.Consul
{
	public class ConsulRegisterServiceConfiguration : IServiceRegisterConfiguration
	{
		public string Id { get; set; }

		public string Name { get; set; }

		public bool EnableTagOverride { get; set; }

		public Dictionary<string, string> Meta { get; set; }

		public string[] Tags { get; set; }

		public TimeSpan HealthCheckInterval { get; set; } = TimeSpan.FromSeconds(10);
	}
}
