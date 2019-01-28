using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Uragano.Abstractions.Exceptions;
using Uragano.Abstractions.ServiceInvoker;
using Microsoft.Extensions.DependencyInjection;
using Uragano.Abstractions;
using Uragano.DynamicProxy.Interceptor;
using ServiceDescriptor = Uragano.Abstractions.ServiceDescriptor;

namespace Uragano.DynamicProxy
{
	public class InvokerFactory : IInvokerFactory
	{
		private static readonly ConcurrentDictionary<string, ServiceDescriptor>
			ServiceInvokers = new ConcurrentDictionary<string, ServiceDescriptor>();


		private IServiceProvider ServiceProvider { get; }
		public InvokerFactory(IServiceProvider serviceProvider)
		{
			ServiceProvider = serviceProvider;
		}

		public void Create(string route, Type interfaceType, MethodInfo methodInfo, List<Type> interceptorTypes)
		{
			route = route.ToLower();
			if (ServiceInvokers.ContainsKey(route))
				throw new DuplicateRouteException(route);
			ServiceInvokers.TryAdd(route, new ServiceDescriptor
			{
				Route = route,
				InterfaceType = interfaceType,
				MethodInfo = methodInfo,
				MethodInvoker = new MethodInvoker(methodInfo),
				Interceptors = interceptorTypes
			});
		}

		public ServiceDescriptor Get(string route)
		{
			if (ServiceInvokers.TryGetValue(route.ToLower(), out var value))
				return value;
			throw new NotFoundRouteException(route);
		}


		public async Task<object> Invoke(string route, object[] args, Dictionary<string, string> meta)
		{
			if (!ServiceInvokers.TryGetValue(route, out var service))
				throw new NotFoundRouteException(route);
			using (var scope = ServiceProvider.CreateScope())
			{
				var context = new InterceptorContext
				{
					ServiceRoute = service.Route,
					ServiceProvider = scope.ServiceProvider,
					Args = args,
					Meta = meta
				};
				context.Interceptors.Push(typeof(ServerDefaultInterceptor));
				foreach (var interceptor in service.Interceptors)
				{
					context.Interceptors.Push(interceptor);
				}

				var result = await ((IInterceptor)scope.ServiceProvider.GetRequiredService(context.Interceptors.Pop()))
					.Intercept(context);
				return result;
			}
		}
	}
}
