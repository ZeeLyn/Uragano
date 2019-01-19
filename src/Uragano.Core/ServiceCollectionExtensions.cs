using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using Uragano.Abstractions;
using Uragano.Abstractions.ServiceInvoker;
using Uragano.DynamicProxy;
using Uragano.Remoting;

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

			var config = new UraganoConfiguration { ServiceCollection = serviceCollection };
			configuration(config);
			serviceCollection.AddSingleton(config.UraganoSettings);
			return serviceCollection;
		}

		public static IServiceCollection AddUraganoServer(this IServiceCollection serviceCollection)
		{
			serviceCollection.AddSingleton<IServiceProxy, ServiceProxy>();
			serviceCollection.AddSingleton<IServiceBuilder, ServiceBuilder>();
			serviceCollection.AddSingleton<IInvokerFactory, InvokerFactory>();
			serviceCollection.AddSingleton<IProxyGenerator, ProxyGenerator>();
			serviceCollection.AddSingleton<IProxyGenerateFactory, ProxyGenerateFactory>();

			#region server
			serviceCollection.AddSingleton<IBootstrap, ServerBootstrap>();
			#endregion

			RegisterServiceAndInterceptor(serviceCollection, true);
			return serviceCollection;
		}

		public static IServiceCollection AddUraganoClient(this IServiceCollection serviceCollection)
		{
			serviceCollection.AddSingleton<IServiceProxy, ServiceProxy>();
			serviceCollection.AddSingleton<IServiceBuilder, ServiceBuilder>();
			serviceCollection.AddSingleton<IInvokerFactory, InvokerFactory>();
			serviceCollection.AddSingleton<IProxyGenerator, ProxyGenerator>();
			serviceCollection.AddSingleton<IProxyGenerateFactory, ProxyGenerateFactory>();

			serviceCollection.AddSingleton<IClientFactory, ClientFactory>();



			RegisterServiceAndInterceptor(serviceCollection);
			return serviceCollection;
		}

		private static void RegisterServiceAndInterceptor(IServiceCollection serviceCollection, bool isServer = false)
		{
			var ignoreAssemblyFix = new[]
			{
				"Microsoft", "System", "Consul", "Polly", "Newtonsoft.Json", "MessagePack", "Google.Protobuf",
				"Remotion.Linq", "SOS.NETCore", "WindowsBase", "mscorlib", "netstandard", "Uragano"
			};

			var assemblies = DependencyContext.Default.RuntimeLibraries.SelectMany(i =>
				i.GetDefaultAssemblyNames(DependencyContext.Default)
					.Where(p => !ignoreAssemblyFix.Any(ignore =>
						p.Name.StartsWith(ignore, StringComparison.CurrentCultureIgnoreCase)))
					.Select(z => Assembly.Load(new AssemblyName(z.Name)))).Where(p => !p.IsDynamic).ToList();

			var types = assemblies.SelectMany(p => p.GetExportedTypes()).ToList();
			var services = types.Where(t => t.IsInterface && typeof(IService).IsAssignableFrom(t)).Select(@interface => new
			{
				Interface = @interface,
				Implementation = types.FirstOrDefault(p => p.IsClass && p.IsPublic && !p.IsAbstract && @interface.IsAssignableFrom(p))
			});
			if (isServer)
			{
				foreach (var service in services)
				{
					serviceCollection.AddTransient(service.Interface, service.Implementation);
				}
			}

			var interceptors = types.FindAll(t => typeof(IInterceptor).IsAssignableFrom(t));
			foreach (var interceptor in interceptors)
			{
				serviceCollection.AddScoped(interceptor);
			}
		}
	}
}
