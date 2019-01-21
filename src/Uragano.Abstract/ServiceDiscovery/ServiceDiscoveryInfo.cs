using System.Collections.Generic;

namespace Uragano.Abstractions.ServiceDiscovery
{
	public class ServiceDiscoveryInfo
	{
		public string ServiceId { get; set; }
		public string Address { get; set; }

		public int Port { get; set; }

		public IDictionary<string, string> Meta { get; set; }

		public bool Alive { get; set; }
	}

	public class ServiceNodeInfo : ServiceDiscoveryInfo
	{
		public int Weight { get; set; }

		public int CurrentWeight { get; set; }
	}
}
