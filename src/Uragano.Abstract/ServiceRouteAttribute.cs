using System;

namespace Uragano.Abstractions
{
	[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method)]
	public class ServiceRouteAttribute : Attribute
	{
		public string Route { get; set; }

		public ServiceRouteAttribute(string route)
		{
			Route = route;
		}
	}
}
