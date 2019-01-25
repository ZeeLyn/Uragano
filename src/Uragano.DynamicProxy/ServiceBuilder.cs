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
				var routeAttr = service.Interface.GetCustomAttribute<ServiceRouteAttribute>();

				var routePrefix = routeAttr == null ? $"{service.Interface.Namespace}/{service.Interface.Name}" : routeAttr.Route;
				var interfaceMethods = service.Interface.GetMethods();
				var implementationMethods = service.Implementation.GetMethods();
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

					InvokerFactory.Create(route, service.Interface, implementationMethods.First(p => IsImplementationMethod(method, p)), interceptors);
				}
			}
		}

		/// <summary>
		/// TODO方法筛选有bug，可能有同名的
		/// </summary>
		/// <param name="serviceMethod"></param>
		/// <param name="implementationMethod"></param>
		/// <returns></returns>
		private bool IsImplementationMethod(MethodInfo serviceMethod, MethodInfo implementationMethod)
		{
			return serviceMethod.Name == implementationMethod.Name &&
				   serviceMethod.ReturnType == implementationMethod.ReturnType &&
				   serviceMethod.ContainsGenericParameters == implementationMethod.ContainsGenericParameters &&
				   SameParameters(serviceMethod.GetParameters(), implementationMethod.GetParameters());
		}

		/// <summary>
		/// 需要判断参数顺序
		/// </summary>
		/// <param name="parameters1"></param>
		/// <param name="parameters2"></param>
		/// <returns></returns>
		private bool SameParameters(ParameterInfo[] parameters1, ParameterInfo[] parameters2)
		{
			if (parameters1.Length == parameters2.Length)
			{
				return !parameters1.Where((t, i) => t.ParameterType != parameters2[i].ParameterType || t.IsOptional != parameters2[i].IsOptional).Any();
			}
			return false;
		}
	}
}
