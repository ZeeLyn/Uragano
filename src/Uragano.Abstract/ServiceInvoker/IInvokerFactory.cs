using System;
using System.Collections.Generic;
using System.Reflection;

namespace Uragano.Abstractions.ServiceInvoker
{
	public interface IInvokerFactory
	{
		void Create(string serviceName, string route, MethodInfo methodInfo, IEnumerable<Type> interceptors);
		ServiceDescriptor Get(string route);

		ServiceDescriptor Get(MethodInfo methodInfo);
	}
}
