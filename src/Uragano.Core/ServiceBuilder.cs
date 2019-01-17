using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyModel;
using Uragano.Abstractions;
using Uragano.Abstractions.ServiceInvoker;
using Uragano.DynamicProxy;

namespace Uragano.Core
{
	public class ServiceBuilder : IServiceBuilder
	{
		private IProxyGenerateFactory ServiceProxyFactory { get; }
		private IInvokerFactory InvokerFactory { get; }

		private IServiceProvider ServiceProvider { get; }

		public ServiceBuilder(IProxyGenerateFactory serviceProxyFactory, IInvokerFactory invokerFactory, IServiceProvider serviceProvider)
		{
			ServiceProxyFactory = serviceProxyFactory;
			InvokerFactory = invokerFactory;
			ServiceProvider = serviceProvider;
		}

		public void BuildServer()
		{
			ContainerManager.Init(ServiceProvider);
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

			foreach (var service in services)
			{
				ServiceProxyFactory.CreateLocalProxy(service.Interface);
				var routeAttr = service.Interface.GetCustomAttribute<ServiceRouteAttribute>();
				var route = routeAttr == null ? $"{service.Interface.Namespace}/{service.Interface.Name}" : routeAttr.Route;
				var methods = service.Interface.GetMethods();
				var interfaceInterceptors = service.Interface.GetCustomAttributes(true).Where(p => p is IInterceptor)
					.Select(p => p.GetType()).ToList();
				foreach (var method in methods)
				{
					var idAttr = method.GetCustomAttribute<ServiceRouteAttribute>();
					route = idAttr == null ? $"{route}/{method.Name}" : $"{route}/{idAttr.Route}";
					var interceptors = method.GetCustomAttributes(true)
						.Where(p => p is IInterceptor).Select(p => p.GetType()).ToList();
					interceptors.AddRange(interfaceInterceptors);
					interceptors.Reverse();
					InvokerFactory.Create(route, method, interceptors);
				}
			}
		}
	}
}
