using System;
using System.Collections.Generic;
using System.Text;

namespace Uragano.Abstractions.ServiceDiscovery
{
	public interface IServiceRegisterConfiguration
	{
		int? Weight { get; set; }
	}
}
