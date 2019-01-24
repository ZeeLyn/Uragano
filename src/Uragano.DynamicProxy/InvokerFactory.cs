using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using Uragano.Abstractions;
using Uragano.Abstractions.Exceptions;
using Uragano.Abstractions.ServiceInvoker;

namespace Uragano.DynamicProxy
{
	public class InvokerFactory : IInvokerFactory
	{
		private static readonly ConcurrentDictionary<string, ServiceDescriptor>
			ServiceInvokers = new ConcurrentDictionary<string, ServiceDescriptor>();

		private static readonly ConcurrentDictionary<MethodInfo, string>
			MethodMapRoute = new ConcurrentDictionary<MethodInfo, string>();

		public void Create(string route, Type implementationType, MethodInfo methodInfo, IEnumerable<Type> interceptorTypes)
		{
			if (ServiceInvokers.ContainsKey(route))
				throw new Exception();
			ServiceInvokers.TryAdd(route, new ServiceDescriptor
			{
				Route = route,
				MethodInfo = methodInfo,
				MethodInvoker = new MethodInvoker(methodInfo),
				Interceptors = interceptorTypes
			});
			MethodMapRoute.TryAdd(methodInfo, route.ToLower());
		}

		public ServiceDescriptor Get(string route)
		{
			if (ServiceInvokers.TryGetValue(route.ToLower(), out var value))
				return value;
			throw new NotFoundRouteException(route);
		}

		public ServiceDescriptor Get(MethodInfo methodInfo)
		{
			if (!MethodMapRoute.TryGetValue(methodInfo, out var route)) throw new NotFoundRouteException("");
			if (ServiceInvokers.TryGetValue(route, out var serviceDescriptor))
			{
				return serviceDescriptor;
			}
			throw new NotFoundRouteException(route);
		}
	}
}
