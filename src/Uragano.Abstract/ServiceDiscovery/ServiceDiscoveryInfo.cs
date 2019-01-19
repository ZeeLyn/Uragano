using System;
using System.Collections.Generic;
using System.Text;

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
}
