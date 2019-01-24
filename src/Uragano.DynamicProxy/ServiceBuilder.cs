using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyModel;
using Uragano.Abstractions;
using Uragano.Abstractions.ServiceInvoker;

namespace Uragano.DynamicProxy
{
	public class ServiceBuilder : IServiceBuilder
	{
		private IInvokerFactory InvokerFactory { get; }

		private IServiceProvider ServiceProvider { get; }

		public ServiceBuilder(IInvokerFactory invokerFactory, IServiceProvider serviceProvider)
		{
			InvokerFactory = invokerFactory;
			ServiceProvider = serviceProvider;
		}

		public void BuildService()
		{
			var types = ReflectHelper.GetDependencyTypes();
			var services = types.Where(t => t.IsInterface && typeof(IService).IsAssignableFrom(t)).Select(@interface => new
			{
				Interface = @interface,
				Implementation = types.FirstOrDefault(p => p.IsClass && p.IsPublic && !p.IsAbstract && @interface.IsAssignableFrom(p))
			}).Where(p => p.Implementation != null);

			foreach (var service in services)
			{
				//var serviceNameAttr = service.Interface.GetCustomAttribute<ServiceDiscoveryNameAttribute>();
				//if (serviceNameAttr == null)
				//	throw new InvalidOperationException($"Interface {service.Interface.FullName} must add a custom attribute ServiceNameAttribute.");

				//ServiceProxyFactory.CreateLocalProxy(service.Interface);
				var routeAttr = service.Interface.GetCustomAttribute<ServiceRouteAttribute>();

				var routePrefix = routeAttr == null ? $"{service.Interface.Namespace}/{service.Interface.Name}" : routeAttr.Route;
				var interfaceMethods = service.Interface.GetMethods();
				var implementationMethods = service.Implementation.GetMethods(BindingFlags.Public);
				var interfaceInterceptors = service.Interface.GetCustomAttributes(true).Where(p => p is IInterceptor)
					.Select(p => p.GetType()).ToList();

				foreach (var method in interfaceMethods)
				{
					var idAttr = method.GetCustomAttribute<ServiceRouteAttribute>();
					var route = idAttr == null ? $"{routePrefix}/{method.Name}" : $"{routePrefix}/{idAttr.Route}";
					var interceptors = method.GetCustomAttributes(true)
						.Where(p => p is IInterceptor).Select(p => p.GetType()).ToList();
					interceptors.AddRange(interfaceInterceptors);
					interceptors.Reverse();
					//方法筛选有bug，可能有同名的
					InvokerFactory.Create(route.ToLower(), service.Implementation, implementationMethods.First(p => p.Name == method.Name), interceptors);
				}
			}
		}
	}
}
