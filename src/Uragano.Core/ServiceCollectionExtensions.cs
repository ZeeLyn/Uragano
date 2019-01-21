using System;
using Microsoft.Extensions.DependencyInjection;
using Uragano.Abstractions;
using Uragano.Abstractions.LoadBalancing;
using Uragano.Abstractions.ServiceInvoker;
using Uragano.DynamicProxy;

namespace Uragano.Core
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddUragano(this IServiceCollection serviceCollection, Action<IUraganoConfiguration> configuration)
		{
			#region register base service
			serviceCollection.AddSingleton<IServiceProxy, ServiceProxy>();
			serviceCollection.AddSingleton<IServiceBuilder, ServiceBuilder>();
			serviceCollection.AddSingleton<IInvokerFactory, InvokerFactory>();
			serviceCollection.AddSingleton<IProxyGenerator, ProxyGenerator>();
			serviceCollection.AddSingleton<IProxyGenerateFactory, ProxyGenerateFactory>();
			#endregion

			var config = new UraganoConfiguration(serviceCollection);
			configuration(config);
			Console.WriteLine("exec adduragano -------->");
			serviceCollection.AddSingleton(config.UraganoSettings);
			if (config.UraganoSettings.ClientInvokeServices != null)
				serviceCollection.AddSingleton(typeof(ILoadBalancing), UraganoOptions.Client_LoadBalancing.Value);
			return serviceCollection;
		}
	}
}
