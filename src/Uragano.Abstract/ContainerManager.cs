using System;
using Microsoft.Extensions.DependencyInjection;

namespace Uragano.Abstractions
{
	public class ContainerManager
	{
		private static IServiceProvider _serviceProvider { get; set; }

		public static void Init(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		public static IServiceProvider ServiceProvider()
		{
			return _serviceProvider;
		}

		public static IServiceScope CreateScope()
		{
			return _serviceProvider.CreateScope();
		}
	}
}
