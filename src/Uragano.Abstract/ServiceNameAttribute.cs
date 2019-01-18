using System;

namespace Uragano.Abstractions
{
	[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method)]
	public class ServiceNameAttribute : Attribute
	{
		public ServiceNameAttribute(string name)
		{
			Name = name;
		}

		public string Name { get; set; }
	}
}
